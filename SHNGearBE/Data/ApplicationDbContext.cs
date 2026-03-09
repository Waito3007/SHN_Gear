using Microsoft.EntityFrameworkCore;
using SHNGearBE.Models.Entities;
using SHNGearBE.Models.Entities.Account;
using SHNGearBE.Models.Entities.Product;
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

    #region Product

    public DbSet<Brand> Brands { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductVariantPrice> ProductVariantPrices { get; set; }
    public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }

    #endregion Product

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

}
