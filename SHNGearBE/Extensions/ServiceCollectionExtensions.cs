using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;

namespace SHNGearBE.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

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
