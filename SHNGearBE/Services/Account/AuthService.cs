using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SHNGearBE.Configurations;
using SHNGearBE.Models.DTOs.Account;
using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Account;
using SHNGearBE.Repositorys.Interface.Role;
using SHNGearBE.Repositorys.Interface.Permission;
using SHNGearBE.Repositorys.Interface.RefreshToken;
using SHNGearBE.Infrastructure.Redis;
using SHNGearBE.Services.Interfaces.Account;
using SHNGearBE.UnitOfWork;
using SHNGearMailService.Abstractions;
using SHNGearMailService.Models;

namespace SHNGearBE.Services.Account;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateRenderer _emailTemplateRenderer;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        ICacheService cacheService,
        IEmailService emailService,
        IEmailTemplateRenderer emailTemplateRenderer,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _cacheService = cacheService;
        _emailService = emailService;
        _emailTemplateRenderer = emailTemplateRenderer;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate email uniqueness
        if (await _accountRepository.EmailExistsAsync(request.Email))
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Email already exists");
        }

        // Validate username uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.Username) &&
            await _accountRepository.UsernameExistsAsync(request.Username))
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Username already exists");
        }

        // Hash password
        var (passwordHash, salt) = HashPassword(request.Password);

        // Create account
        var account = new Models.Entities.Account.Account
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Salt = salt,
            CreateAt = DateTime.UtcNow
        };

        // Create account details
        var accountDetail = new AccountDetail
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            FirstName = request.FirstName,
            Name = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            CreateAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _accountRepository.AddAsync(account);
            account.AccountDetail = accountDetail;

            // Assign default "User" role
            var userRole = await _roleRepository.GetByNameAsync("User");
            if (userRole != null)
            {
                var accountRole = new AccountRole
                {
                    AccountId = account.Id,
                    RoleId = userRole.Id
                };
                account.AccountRoles = new List<AccountRole> { accountRole };
            }

            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            await _cacheService.SetAsync(GetEmailVerificationRequiredKey(account.Email), true, TimeSpan.FromDays(30));
            await SendEmailVerificationOtpAsync(account.Email);

            return await GenerateLoginResponse(account.Id);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Find account by email or username
        var account = await _accountRepository.GetByEmailAsync(request.EmailOrUsername)
            ?? await _accountRepository.GetByUsernameAsync(request.EmailOrUsername);

        if (account == null)
        {
            throw new ProjectException(ResponseType.Unauthorized, "Invalid credentials");
        }

        var verificationRequired = await _cacheService.GetAsync<bool>(GetEmailVerificationRequiredKey(account.Email));
        if (verificationRequired)
        {
            throw new ProjectException(ResponseType.Forbidden, "Email verification is required");
        }

        // Verify password
        if (!VerifyPassword(request.Password, account.PasswordHash, account.Salt))
        {
            throw new ProjectException(ResponseType.Unauthorized, "Invalid credentials");
        }

        return await GenerateLoginResponse(account.Id);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        // Get principal from expired access token
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new ProjectException(ResponseType.Unauthorized, "Invalid access token");
        }

        // Get refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.JwtToken != request.AccessToken)
        {
            throw new ProjectException(ResponseType.Unauthorized, "Invalid refresh token");
        }

        // Validate refresh token
        if (refreshToken.IsUsed || refreshToken.IsRevoked)
        {
            throw new ProjectException(ResponseType.Unauthorized, "Refresh token has been used or revoked");
        }

        if (refreshToken.Expires < DateTime.UtcNow)
        {
            throw new ProjectException(ResponseType.Unauthorized, "Refresh token has expired");
        }

        // Mark old refresh token as used
        refreshToken.IsUsed = true;
        refreshToken.UpdateAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken);
        await _unitOfWork.SaveAsync();

        return await GenerateLoginResponse(refreshToken.AccountId);
    }

    public async Task RevokeTokenAsync(string token)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (refreshToken == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Refresh token not found");
        }

        await _refreshTokenRepository.RevokeTokenAsync(refreshToken.Id);
        await _unitOfWork.SaveAsync();
    }

    public async Task LogoutAsync(Guid accountId)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(accountId);
        await _unitOfWork.SaveAsync();
    }

    public async Task<bool> ChangePasswordAsync(Guid accountId, ChangePasswordRequestDto request)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        // Verify current password
        if (!VerifyPassword(request.CurrentPassword, account.PasswordHash, account.Salt))
        {
            throw new ProjectException(ResponseType.BadRequest, "Current password is incorrect");
        }

        // Hash new password
        var (passwordHash, salt) = HashPassword(request.NewPassword);

        account.PasswordHash = passwordHash;
        account.Salt = salt;
        account.UpdateAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        await _unitOfWork.SaveAsync();

        // Revoke all existing tokens
        await _refreshTokenRepository.RevokeAllUserTokensAsync(accountId);
        await _unitOfWork.SaveAsync();

        return true;
    }

    public async Task SendEmailVerificationOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ProjectException(ResponseType.InvalidData, "Email is required");
        }

        var account = await _accountRepository.GetByEmailAsync(email.Trim().ToLowerInvariant());
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        var otp = GenerateOtp();
        var otpKey = GetEmailVerificationOtpKey(account.Email);
        var attemptKey = GetOtpAttemptKey(otpKey);

        await _cacheService.SetAsync(otpKey, otp, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(attemptKey, 0, TimeSpan.FromMinutes(5));

        var rendered = _emailTemplateRenderer.RenderOtpTemplate(
            OtpEmailTemplateType.VerifyEmail,
            new OtpEmailTemplateData
            {
                RecipientName = account.Username ?? account.Email,
                OtpCode = otp,
                ExpiryMinutes = 5
            });

        var emailMessage = new EmailMessage
        {
            Subject = rendered.Subject,
            Body = rendered.HtmlBody,
            IsHtml = true,
            To =
            {
                new EmailAddress { Address = account.Email, DisplayName = account.Username }
            }
        };

        var sendResult = await _emailService.SendAsync(emailMessage, cancellationToken);
        if (!sendResult.Success)
        {
            throw new ProjectException(ResponseType.ServiceUnavailable, sendResult.ErrorMessage ?? "Unable to send OTP email");
        }
    }

    public async Task<bool> VerifyEmailOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        ValidateOtpInput(otp);

        var account = await _accountRepository.GetByEmailAsync(normalizedEmail);
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        var otpKey = GetEmailVerificationOtpKey(normalizedEmail);
        var attemptKey = GetOtpAttemptKey(otpKey);
        await EnsureNotLockedAsync(attemptKey);

        var cachedOtp = await _cacheService.GetAsync<string>(otpKey);
        if (string.IsNullOrWhiteSpace(cachedOtp) || !string.Equals(cachedOtp, otp.Trim(), StringComparison.Ordinal))
        {
            await IncreaseAttemptAsync(attemptKey);
            throw new ProjectException(ResponseType.InvalidData, "OTP is invalid or expired");
        }

        await _cacheService.RemoveAsync(otpKey);
        await _cacheService.RemoveAsync(attemptKey);
        await _cacheService.SetAsync(GetEmailVerifiedKey(normalizedEmail), true, TimeSpan.FromDays(3650));
        await _cacheService.RemoveAsync(GetEmailVerificationRequiredKey(normalizedEmail));

        return true;
    }

    public async Task SendForgotPasswordOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ProjectException(ResponseType.InvalidData, "Email is required");
        }

        var normalizedEmail = NormalizeEmail(email);
        var account = await _accountRepository.GetByEmailAsync(normalizedEmail);
        if (account == null)
        {
            return;
        }

        var otp = GenerateOtp();
        var otpKey = GetForgotPasswordOtpKey(normalizedEmail);
        var attemptKey = GetOtpAttemptKey(otpKey);

        await _cacheService.SetAsync(otpKey, otp, TimeSpan.FromMinutes(5));
        await _cacheService.SetAsync(attemptKey, 0, TimeSpan.FromMinutes(5));

        var rendered = _emailTemplateRenderer.RenderOtpTemplate(
            OtpEmailTemplateType.ForgotPassword,
            new OtpEmailTemplateData
            {
                RecipientName = account.Username ?? account.Email,
                OtpCode = otp,
                ExpiryMinutes = 5
            });

        var emailMessage = new EmailMessage
        {
            Subject = rendered.Subject,
            Body = rendered.HtmlBody,
            IsHtml = true,
            To =
            {
                new EmailAddress { Address = account.Email, DisplayName = account.Username }
            }
        };

        var sendResult = await _emailService.SendAsync(emailMessage, cancellationToken);
        if (!sendResult.Success)
        {
            throw new ProjectException(ResponseType.ServiceUnavailable, sendResult.ErrorMessage ?? "Unable to send OTP email");
        }
    }

    public async Task<VerifyForgotPasswordOtpResponseDto> VerifyForgotPasswordOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        ValidateOtpInput(otp);

        var otpKey = GetForgotPasswordOtpKey(normalizedEmail);
        var attemptKey = GetOtpAttemptKey(otpKey);

        await EnsureNotLockedAsync(attemptKey);

        var cachedOtp = await _cacheService.GetAsync<string>(otpKey);
        if (string.IsNullOrWhiteSpace(cachedOtp) || !string.Equals(cachedOtp, otp.Trim(), StringComparison.Ordinal))
        {
            await IncreaseAttemptAsync(attemptKey);
            throw new ProjectException(ResponseType.InvalidData, "OTP is invalid or expired");
        }

        await _cacheService.RemoveAsync(otpKey);
        await _cacheService.RemoveAsync(attemptKey);

        var verificationToken = Guid.NewGuid().ToString("N");
        var tokenKey = GetForgotPasswordTokenKey(verificationToken);
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        await _cacheService.SetAsync(tokenKey, normalizedEmail, TimeSpan.FromMinutes(10));

        return new VerifyForgotPasswordOtpResponseDto
        {
            VerificationToken = verificationToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> ResetForgotPasswordAsync(ResetForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(request.VerificationToken))
        {
            throw new ProjectException(ResponseType.InvalidData, "Verification token is required");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            throw new ProjectException(ResponseType.InvalidData, "New password is invalid");
        }

        var tokenKey = GetForgotPasswordTokenKey(request.VerificationToken.Trim());
        var emailFromToken = await _cacheService.GetAsync<string>(tokenKey);
        if (string.IsNullOrWhiteSpace(emailFromToken) || !string.Equals(emailFromToken, normalizedEmail, StringComparison.Ordinal))
        {
            throw new ProjectException(ResponseType.InvalidData, "Verification token is invalid or expired");
        }

        var account = await _accountRepository.GetByEmailAsync(normalizedEmail);
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        var (passwordHash, salt) = HashPassword(request.NewPassword);
        account.PasswordHash = passwordHash;
        account.Salt = salt;
        account.UpdateAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        await _refreshTokenRepository.RevokeAllUserTokensAsync(account.Id);
        await _unitOfWork.SaveAsync();

        await _cacheService.RemoveAsync(tokenKey);
        return true;
    }

    private async Task<LoginResponseDto> GenerateLoginResponse(Guid accountId)
    {
        var account = await _accountRepository.GetAccountWithRolesAndPermissionsAsync(accountId);
        if (account == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Account not found");
        }

        var roles = account.AccountRoles?.Select(ar => ar.Role.Name).ToList() ?? new List<string>();
        var permissions = account.AccountRoles?
            .SelectMany(ar => ar.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList() ?? new List<string>();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(account.Id, account.Email, roles, permissions);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Token = refreshTokenValue,
            JwtToken = accessToken,
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            IsUsed = false,
            IsRevoked = false,
            CreateAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _unitOfWork.SaveAsync();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Account = new AccountDto
            {
                Id = account.Id,
                Username = account.Username,
                Email = account.Email,
                FirstName = account.AccountDetail?.FirstName,
                LastName = account.AccountDetail?.Name,
                PhoneNumber = account.AccountDetail?.PhoneNumber,
                Address = account.AccountDetail?.Address,
                Roles = roles,
                Permissions = permissions
            }
        };
    }

    private (string hash, string salt) HashPassword(string password)
    {
        var saltBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        var salt = Convert.ToBase64String(saltBytes);
        var hash = HashPasswordWithSalt(password, salt);

        return (hash, salt);
    }

    private bool VerifyPassword(string password, string hash, string salt)
    {
        var computedHash = HashPasswordWithSalt(password, salt);
        return computedHash == hash;
    }

    private string HashPasswordWithSalt(string password, string salt)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(salt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ProjectException(ResponseType.InvalidData, "Email is required");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static void ValidateOtpInput(string otp)
    {
        if (string.IsNullOrWhiteSpace(otp) || otp.Trim().Length != 6)
        {
            throw new ProjectException(ResponseType.InvalidData, "OTP must be 6 digits");
        }
    }

    private static string GenerateOtp()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }

    private async Task EnsureNotLockedAsync(string attemptKey)
    {
        var attempts = await _cacheService.GetAsync<int>(attemptKey);
        if (attempts >= 5)
        {
            throw new ProjectException(ResponseType.Forbidden, "OTP has been locked due to too many attempts");
        }
    }

    private async Task IncreaseAttemptAsync(string attemptKey)
    {
        var attempts = await _cacheService.GetAsync<int>(attemptKey);
        attempts++;
        await _cacheService.SetAsync(attemptKey, attempts, TimeSpan.FromMinutes(5));
    }

    private static string GetEmailVerificationOtpKey(string email)
        => $"auth:otp:verify-email:{email}";

    private static string GetForgotPasswordOtpKey(string email)
        => $"auth:otp:forgot-password:{email}";

    private static string GetOtpAttemptKey(string otpKey)
        => $"{otpKey}:attempts";

    private static string GetForgotPasswordTokenKey(string token)
        => $"auth:forgot-password:token:{token}";

    private static string GetEmailVerifiedKey(string email)
        => $"auth:email-verified:{email}";

    private static string GetEmailVerificationRequiredKey(string email)
        => $"auth:email-verification-required:{email}";
}
