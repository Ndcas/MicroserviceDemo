using System.Text.Json.Serialization;

namespace Frontend.Dtos;

public record AuthenticationData([property: JsonPropertyName("accessToken")] string AccessToken);