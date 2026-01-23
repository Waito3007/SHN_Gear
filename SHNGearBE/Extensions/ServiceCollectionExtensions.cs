using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Repositorys;
using SHNGearBE.Repositorys.Interface;
using SHNGearBE.Services;
using SHNGearBE.Services.Interfaces;
using SHNGearBE.UnitOfWork;

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

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, SHNGearBE.UnitOfWork.UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }

    // Uncomment when Redis is configured
    // public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.Configure<RedisConfiguration>(configuration.GetSection(RedisConfiguration.SectionName));
    //     services.AddSingleton<ICacheService, RedisCacheService>();
    //     return services;
    // }

    // Uncomment when RabbitMQ is configured
    // public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    // {
    //     services.Configure<RabbitMQConfiguration>(configuration.GetSection(RabbitMQConfiguration.SectionName));
    //     services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
    //     return services;
    // }
}
