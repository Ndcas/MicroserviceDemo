using System.Text.Json.Serialization;

namespace Frontend.Dtos;

public record OrderListData([property: JsonPropertyName("total")] int Total, [property: JsonPropertyName("orders")] IReadOnlyList<OrderData> Orders);