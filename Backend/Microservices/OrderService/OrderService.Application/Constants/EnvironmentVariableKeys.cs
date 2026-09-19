namespace OrderService.Application.Constants;

internal static class EnvironmentVariableKeys
{
    public const string ProductServiceApiUrl = "ServiceApiUrl:ProductService";
    public const string OrderCanceledTopic = "Broker:OrderCanceledTopic";
    public const string OrderCreatedTopic = "Broker:OrderCreatedTopic";
    public const string PaymentCompletedTopic = "Broker:PaymentCompletedTopic";
}
