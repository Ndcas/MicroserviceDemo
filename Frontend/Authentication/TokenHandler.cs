using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Frontend.Constants;
using Frontend.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

namespace Frontend.Api;

internal class TokenHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigationManager;

    public TokenHandler(ILocalStorageService localStorageService, IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        _localStorageService = localStorageService;
        _httpClientFactory = httpClientFactory;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var accessToken = _localStorageService.GetItem<string>(LocalStorageKeys.AccessToken);

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var authClient = _httpClientFactory.CreateClient(HttpClients.RefreshClientName);

            using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, UserServiceEndpoints.RefreshAccessToken);

            refreshRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var refreshResponse = await authClient.SendAsync(refreshRequest, cancellationToken);

            if (!refreshResponse.IsSuccessStatusCode)
            {
                _localStorageService.RemoveItem(LocalStorageKeys.AccessToken);

                _navigationManager.NavigateTo(Routes.Login);

                return response;
            }

            var data = await refreshResponse.Content.ReadFromJsonAsync<AuthenticationData>(cancellationToken);

            _localStorageService.SetItem<string>(LocalStorageKeys.AccessToken, data.AccessToken);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.AccessToken);

            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }
}
