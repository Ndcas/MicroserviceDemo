namespace UserService.Infrastructure.Constants;

internal static class EnvironmentVariableKeys
{
    public const string DatabaseConnectionString = "DefaultConnection";
    public const string PulsarConnectionString = "Pulsar";
    public const string RedisConnectionString = "Redis";
    public const string BrokerLogSentTopic = "Broker:LogSentTopic";
}

