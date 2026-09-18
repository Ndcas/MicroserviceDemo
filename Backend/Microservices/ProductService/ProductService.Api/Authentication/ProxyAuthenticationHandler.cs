using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ProductService.Api.Constants;

namespace ProductService.Api.Authentication;

internal class ProxyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ProxyHeaderAuthentication";

    public ProxyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ProxyHeaders.UserId, out var userId) ||
            !Request.Headers.TryGetValue(ProxyHeaders.UserName, out var name) ||
            !Request.Headers.TryGetValue(ProxyHeaders.RoleId, out var roleId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new Claim(JwtConfigurations.ClaimTypeUserId, userId),
            new Claim(JwtConfigurations.ClaimTypeName, name),
            new Claim(JwtConfigurations.ClaimTypeRoleId, roleId)

        };

        var identity = new ClaimsIdentity(claims, Scheme.Name, JwtConfigurations.ClaimTypeName, JwtConfigurations.ClaimTypeRoleId);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
