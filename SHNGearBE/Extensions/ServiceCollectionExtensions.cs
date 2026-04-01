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
using SHNGearBE.Repositorys.Address;
using SHNGearBE.Repositorys.Interface.Address;
using SHNGearBE.Repositorys.Order;
using SHNGearBE.Repositorys.Interface.Order;
using SHNGearBE.Services.Account;
using SHNGearBE.Services.Role;
using SHNGearBE.Services.Permission;
using SHNGearBE.Services.Order;
using SHNGearBE.Services;
using SHNGearBE.Services.Interfaces.Account;
using SHNGearBE.Services.Interfaces.Role;
using SHNGearBE.Services.Interfaces.Permission;
using SHNGearBE.Services.Interfaces;
using SHNGearBE.Services.Interfaces.Address;
using SHNGearBE.Services.Interfaces.Cart;
using SHNGearBE.Services.Interfaces.Order;
using SHNGearBE.Services.Cart;
using SHNGearBE.UnitOfWork;
using SHNGearBE.Configurations;
using SHNGearBE.Infrastructure.Redis;
using SHNGearBE.Infrastructure.Media;
using SHNGearBE.Infrastructure.Payment;
using SHNGearBE.Services.Interfaces.Media;
using SHNGearBE.Services.Interfaces.Product;

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
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
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
        services.AddScoped<IProductMetaService, ProductMetaService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // Address services
        services.AddScoped<IAddressService, SHNGearBE.Services.Address.AddressService>();

        // Cart services (Redis-based)
        services.AddScoped<ICartService, CartService>();

        // Order services
        services.AddScoped<IOrderService, OrderService>();

        // Payment strategies (Strategy pattern)
        services.AddScoped<IPaymentStrategy, CodPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, PayPalPaymentStrategy>();
        services.AddScoped<IPaymentStrategyResolver, PaymentStrategyResolver>();

        // Media services (Cloudinary)
        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();

        return services;
    }

    public static IServiceCollection AddPayPalSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PayPalSettings>(configuration.GetSection(PayPalSettings.SectionName));

        services.AddHttpClient<IPayPalGatewayService, PayPalGatewayService>((serviceProvider, client) =>
        {
            var settings = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<PayPalSettings>>()
                .Value;

            var baseUrl = string.IsNullOrWhiteSpace(settings.BaseUrl)
                ? "https://api-m.sandbox.paypal.com"
                : settings.BaseUrl;

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds <= 0 ? 20 : settings.HttpTimeoutSeconds);
        });

        return services;
    }

    public static IServiceCollection AddCloudinarySettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CloudinarySettings>(options =>
        {
            configuration.GetSection(CloudinarySettings.SectionName).Bind(options);

            if (string.IsNullOrWhiteSpace(options.CloudName))
            {
                options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
            }

            if (string.IsNullOrWhiteSpace(options.ApiSecret))
            {
                options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
            }
        });

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
