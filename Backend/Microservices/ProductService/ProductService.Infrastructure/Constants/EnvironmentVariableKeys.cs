namespace ProductService.Infrastructure.Constants;

internal static class EnvironmentVariableKeys
{
    public const string DatabaseConnectionString = "DefaultConnection";
    public const string PulsarConnectionString = "Pulsar";
    public const string BrokerSubscriptionName = "Broker:SubscriptionName";
    public const string BrokerLogSentTopic = "Broker:LogSentTopic";
    public const string OrderCreatedTopic = "Broker:OrderCreatedTopic";
    public const string ReservationCompletedTopic = "Broker:ReservationCompletedTopic";
    public const string ReservationFailedTopic = "Broker:ReservationFailedTopic";
    public const string PaymentCompletedTopic = "Broker:PaymentCompletedTopic";
    public const string OrderCanceledTopic = "Broker:OrderCanceledTopic";
}

