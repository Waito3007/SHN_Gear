using Microsoft.EntityFrameworkCore;
using SHNGearBE.Models.Entities;
using SHNGearBE.Models.Entities.Account;
namespace SHNGearBE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    #region Account

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountDetail> AccountDetails { get; set; }
    public DbSet<AccountRole> AccountRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    #endregion Account

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

}
