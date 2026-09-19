using System.Net.Http.Json;
using Frontend.Constants;
using Frontend.Dtos;
using Frontend.Interfaces;

namespace Frontend.Api;

internal class UserService : IUserService
{
    private readonly HttpClient _client;

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(HttpClients.BusinessClientName);
    }

    public async Task<ApiResponse<AuthenticationData>> LoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync(UserServiceEndpoints.Login, credentials, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse<AuthenticationData>.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse<AuthenticationData>.Success(await response.Content.ReadFromJsonAsync<AuthenticationData>(cancellationToken));
    }

    public async Task<ApiResponse> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync(UserServiceEndpoints.Logout, null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResponse.Fail(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return ApiResponse.Success();
    }
}
