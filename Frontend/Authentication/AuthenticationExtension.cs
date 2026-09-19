using Frontend.Constants;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Authentication;

internal static class AuthenticationExtension
{
    public static bool IsAuthenticated(this AuthenticationState authState)
    {
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    public static string? GetUserId(this AuthenticationState authState)
    {
        return authState.User.Claims.FirstOrDefault(claim => claim.Type == JwtConfigurations.ClaimTypeUserId)?.Value;
    }

    public static string? GetRoleId(this AuthenticationState authState)
    {
        return authState.User.Claims.FirstOrDefault(claim => claim.Type == JwtConfigurations.ClaimTypeRoleId)?.Value;
    }

    public static string? GetUsername(this AuthenticationState authState)
    {
        return authState.User.Claims.FirstOrDefault(claim => claim.Type == JwtConfigurations.ClaimTypeName)?.Value;
    }
}
