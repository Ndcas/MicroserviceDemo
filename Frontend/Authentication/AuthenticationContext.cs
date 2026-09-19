using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Frontend.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Frontend.Authentication;

internal class AuthenticationContext : AuthenticationStateProvider
{
    private static readonly AuthenticationState _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ILocalStorageService _localStorageService;

    public AuthenticationContext(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = _localStorageService.GetItem<string>(LocalStorageKeys.AccessToken);

            if (string.IsNullOrWhiteSpace(token))
            {
                return _anonymous;
            }

            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                _localStorageService.RemoveItem(LocalStorageKeys.AccessToken);

                return _anonymous;
            }

            var identity = new ClaimsIdentity(
                jwt.Claims,
                JwtConfigurations.AuthType,
                JwtConfigurations.ClaimTypeName,
                JwtConfigurations.ClaimTypeRoleId);

            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return _anonymous;
        }
    }

    public async Task Authenticate(string accessToken)
    {
        _localStorageService.SetItem(LocalStorageKeys.AccessToken, accessToken);

        var authState = await GetAuthenticationStateAsync();

        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public void ClearAuthentication()
    {
        _localStorageService.RemoveItem(LocalStorageKeys.AccessToken);

        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }
}
