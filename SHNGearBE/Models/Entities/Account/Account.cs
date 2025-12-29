using System;

namespace SHNGearBE.Models.Entities.Account;

public class Account : BaseEntity
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }
    public string Email { get; set; }

    public AccountDetail AccountDetail { get; set; }
    public virtual ICollection<AccountRole> AccountRoles { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}