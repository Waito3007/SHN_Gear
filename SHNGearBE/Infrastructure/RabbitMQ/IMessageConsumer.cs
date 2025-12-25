namespace SHNGearBE.Infrastructure.RabbitMQ;

public interface IMessageConsumer
{
    Task StartConsumeAsync<T>(string queueName, Func<T, Task> handler);
    Task StopConsumeAsync();
}
