using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using OrderService.Api.Authentication;
using OrderService.Application;
using OrderService.Infrastructure;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication(builder.Configuration);

builder.Services
    .AddAuthentication(ProxyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ProxyAuthenticationHandler>(ProxyAuthenticationHandler.SchemeName, null);

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
