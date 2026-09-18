namespace LogService.Infrastructure.Constants;

internal static class EnvironmentVariableKeys
{
    public const string DatabaseConnectionString = "DefaultConnection";
    public const string PulsarConnectionString = "Pulsar";
    public const string BrokerSubscriptionName = "Broker:SubscriptionName";
    public const string BrokerTopic = "Broker:Topic";
}

