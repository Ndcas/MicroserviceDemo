namespace OrderService.Application.Constants;

internal static class EnvironmentVariableKeys
{
    public const string ProductServiceInternalApiUrl = "ServiceInternalApiUrl:ProductService";
    public const string OrderCanceledTopic = "Broker:OrderCanceledTopic";
    public const string OrderCreatedTopic = "Broker:OrderCreatedTopic";
    public const string PaymentCompletedTopic = "Broker:PaymentCompletedTopic";
}
