namespace SHNGearBE.Models.DTOs.Account;

public class RegisterRequestDto
{
    public string? Username { get; set; }
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class LoginRequestDto
{
    public string EmailOrUsername { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public AccountDto Account { get; set; } = null!;
}

public class AccountDto
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public IEnumerable<string> Roles { get; set; } = new List<string>();
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

public class UpdateAccountRequestDto
{
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}

public class AssignRoleRequestDto
{
    public Guid AccountId { get; set; }
    public Guid RoleId { get; set; }
}
