using Microsoft.EntityFrameworkCore;
using Moq;
using SHNGearBE.Data;
using SHNGearBE.Infrastructure.Redis;
using SHNGearBE.Repositorys.Product;
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
        var cacheService = new TestCacheService();
        ProductRepository = new ProductRepository(DbContext, cacheService);
        ProductService = new ProductService(ProductRepository, UnitOfWork, Mock.Of<BackgroundLogService.Abstractions.ILogService<ProductService>>());
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

    private sealed class TestCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(false);
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            return Task.CompletedTask;
        }

        public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            return factory();
        }

        public Task<bool> AcquireLockAsync(string key, TimeSpan expiry)
        {
            return Task.FromResult(true);
        }

        public Task ReleaseLockAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}

