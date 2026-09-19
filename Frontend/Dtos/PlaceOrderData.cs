using System.Text.Json.Serialization;

namespace Frontend.Dtos;

public record PlaceOrderData([property: JsonPropertyName("orderId")] int OrderId);
