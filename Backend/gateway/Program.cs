using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gateway.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var frontendUrl = builder.Configuration.GetValue<string>(EnvironmentVariableKeys.FrontEndUrl)!;
var jwtKey = builder.Configuration.GetValue<string>(EnvironmentVariableKeys.JwtKey)!;
var issuer = builder.Configuration.GetValue<string>(EnvironmentVariableKeys.JwtIssuer)!;
var audience = builder.Configuration.GetValue<string>(EnvironmentVariableKeys.JwtAudience)!;

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsConfigurations.PolicyName, policy =>
    {
        policy
            .WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection(EnvironmentVariableKeys.ReverseProxySection))
    .AddTransforms(builderContext =>
{
    builderContext.AddRequestTransform(transformContext =>
    {
        ClaimsPrincipal user = transformContext.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            string userId = user.FindFirstValue(JwtConfigurations.ClaimTypeUserId)!;
            string name = user.FindFirstValue(JwtConfigurations.ClaimTypeName)!;
            string roleId = user.FindFirstValue(JwtConfigurations.ClaimTypeRoleId)!;

            transformContext.ProxyRequest.Headers.Add(ReverseProxyConfigurations.UserIdHeaderName, userId);

            transformContext.ProxyRequest.Headers.Add(ReverseProxyConfigurations.UserNameHeaderName, name);

            transformContext.ProxyRequest.Headers.Add(ReverseProxyConfigurations.UserRoleIdHeaderName, roleId);
        }
        else
        {
            transformContext.ProxyRequest.Headers.Remove(ReverseProxyConfigurations.UserIdHeaderName);

            transformContext.ProxyRequest.Headers.Remove(ReverseProxyConfigurations.UserNameHeaderName);

            transformContext.ProxyRequest.Headers.Remove(ReverseProxyConfigurations.UserRoleIdHeaderName);
        }

        if (transformContext.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
        {
            string[] ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (ips.Length > 0)
            {
                transformContext.ProxyRequest.Headers.Add(ReverseProxyConfigurations.UserIpAddressHeaderName, ips[0].Trim());
            }
        }
        else
        {
            string ip = transformContext.HttpContext.Connection.RemoteIpAddress?.ToString() ??
                ReverseProxyConfigurations.UnknownIpAddressValue;

            transformContext.ProxyRequest.Headers.Add(ReverseProxyConfigurations.UserIpAddressHeaderName, ip);
        }

        transformContext.ProxyRequest.Headers.Add(ReverseProxyConfigurations.CorrelationIdHeaderName, Guid.NewGuid().ToString());

        return ValueTask.CompletedTask;
    });
});

SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = JwtConfigurations.ClaimTypeName,
        RoleClaimType = JwtConfigurations.ClaimTypeRoleId
    };
});

var app = builder.Build();

app.UseRouting();

app.UseCors(CorsConfigurations.PolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
