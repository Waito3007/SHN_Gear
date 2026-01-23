using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Repositorys;
using SHNGearBE.Services;
using SHNGearBE.UnitOfWork;
using Testcontainers.PostgreSql;
using Xunit;

namespace SHNGearBE.Tests.IntegrationTests;

public class ProductIntegrationTestBase : IAsyncLifetime
{
    protected PostgreSqlContainer PostgresContainer { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;
    protected IUnitOfWork UnitOfWork { get; private set; } = null!;
    protected ProductRepository ProductRepository { get; private set; } = null!;
    protected ProductService ProductService { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Create and start PostgreSQL container
        PostgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("shngeartestdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        await PostgresContainer.StartAsync();

        // Create DbContext with container connection string
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(PostgresContainer.GetConnectionString())
            .Options;

        DbContext = new ApplicationDbContext(options);

        // Ensure database is created and migrations are applied
        await DbContext.Database.EnsureCreatedAsync();

        // Initialize repositories and services
        UnitOfWork = new UnitOfWork.UnitOfWork(DbContext);
        ProductRepository = new ProductRepository(DbContext);
        ProductService = new ProductService(UnitOfWork, ProductRepository);
    }

    public async Task DisposeAsync()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }

        if (PostgresContainer != null)
        {
            await PostgresContainer.StopAsync();
            await PostgresContainer.DisposeAsync();
        }
    }

    protected async Task CleanupDatabase()
    {
        // Clear all data from tables for test isolation
        DbContext.Products.RemoveRange(DbContext.Products);
        DbContext.Categories.RemoveRange(DbContext.Categories);
        DbContext.Brands.RemoveRange(DbContext.Brands);
        DbContext.ProductAttributeDefinitions.RemoveRange(DbContext.ProductAttributeDefinitions);
        DbContext.Tags.RemoveRange(DbContext.Tags);

        await DbContext.SaveChangesAsync();
    }
}
