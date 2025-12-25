namespace SHNGearBE.Configurations;

public class RedisConfiguration
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "SHNGear_";
    public int DefaultExpirationMinutes { get; set; } = 30;
}
