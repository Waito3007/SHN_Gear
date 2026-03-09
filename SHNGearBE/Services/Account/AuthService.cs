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
using SHNGearBE.Services.Interfaces.Account;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Services.Account;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
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
}
