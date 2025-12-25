namespace SHNGearBE.Infrastructure.RabbitMQ;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message);
    Task PublishAsync<T>(string queueName, T message);
}
