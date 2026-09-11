namespace UserService.Infrastructure.Constants;

public static class EnvironmentVariableKeys
{
    public const string DatabaseConnectionString = "DefaultConnection";
    public const string PulsarConnectionString = "Pulsar";
    public const string RedisConnectionString = "Redis";
    public const string BrokerLogTopic = "Broker:LogTopic";
}

