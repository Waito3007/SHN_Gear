using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Repositorys;
using SHNGearBE.Repositorys.Interface;
using SHNGearBE.Repositorys.Account;
using SHNGearBE.Repositorys.Role;
using SHNGearBE.Repositorys.Permission;
using SHNGearBE.Repositorys.RefreshToken;
using SHNGearBE.Repositorys.Product;
using SHNGearBE.Repositorys.Interface.Account;
using SHNGearBE.Repositorys.Interface.Role;
using SHNGearBE.Repositorys.Interface.Permission;
using SHNGearBE.Repositorys.Interface.RefreshToken;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Services.Account;
using SHNGearBE.Services.Role;
using SHNGearBE.Services.Permission;
using SHNGearBE.Services;
using SHNGearBE.Services.Interfaces.Account;
using SHNGearBE.Services.Interfaces.Role;
using SHNGearBE.Services.Interfaces.Permission;
using SHNGearBE.Services.Interfaces;
using SHNGearBE.UnitOfWork;
using SHNGearBE.Configurations;
using SHNGearBE.Infrastructure.Redis;

namespace SHNGearBE.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisConfiguration>(configuration.GetSection(RedisConfiguration.SectionName));

        var redisConfig = configuration.GetSection(RedisConfiguration.SectionName).Get<RedisConfiguration>();

        if (!string.IsNullOrEmpty(redisConfig?.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfig.ConnectionString;
                options.InstanceName = redisConfig.InstanceName;
            });
        }
        else
        {
            // Fallback to in-memory cache for development
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, SHNGearBE.UnitOfWork.UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Account services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();

        // Role services
        services.AddScoped<IRoleService, RoleService>();

        // Permission services
        services.AddScoped<IPermissionService, PermissionService>();

        // Product services
        services.AddScoped<IProductService, ProductService>();

        return services;
    }

    // Uncomment when RabbitMQ is configured
    // public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.Configure<RabbitMQConfiguration>(configuration.GetSection(RabbitMQConfiguration.SectionName));
    //     services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
    //     return services;
    // }
}
