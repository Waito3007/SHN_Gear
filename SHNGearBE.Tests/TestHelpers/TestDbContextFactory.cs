using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;

namespace SHNGearBE.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }

    public static ApplicationDbContext CreatePostgresContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
