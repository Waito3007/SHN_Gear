using System;

namespace SHNGearBE.Models.Entities.Account;

public class RefreshToken : BaseEntity
{
    public Guid Id { get; set; }
    public string JwtToken { get; set; }
    public string Token { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime Expires { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; }
}
