using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Yarp.ReverseProxy.Transforms;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

const string CORS_POLICY = "MicroserviceDemoCORS";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string frontendUrl = builder.Configuration.GetValue<string>("FrontendUrl")!;
string jwtKey = builder.Configuration.GetValue<string>("JWT:Key")!;
string issuer = builder.Configuration.GetValue<string>("JWT:Issuer")!;
string audience = builder.Configuration.GetValue<string>("JWT:Audience")!;

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CORS_POLICY, policy =>
    {
        policy.WithOrigins(frontendUrl).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy")).AddTransforms(builderContext =>
{
    builderContext.AddRequestTransform(transformContext =>
    {
        ClaimsPrincipal user = transformContext.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            string userId = user.FindFirstValue("sub")!;
            string name = user.FindFirstValue("name")!;
            string roleId = user.FindFirstValue("role")!;
            string ip = transformContext.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            transformContext.ProxyRequest.Headers.Add("X-User-Id", userId);

            transformContext.ProxyRequest.Headers.Add("X-User-Name", name);

            transformContext.ProxyRequest.Headers.Add("X-User-Role", roleId);
        }

        if (transformContext.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardedFor))
        {
            string[] ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (ips.Length > 0)
            {
                transformContext.ProxyRequest.Headers.Add("X-Ip", ips[0].Trim());
            }
        }
        else
        {
            string ip = transformContext.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Không rõ";

            transformContext.ProxyRequest.Headers.Add("X-Ip", ip);
        }

        transformContext.ProxyRequest.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());

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
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

var app = builder.Build();

app.UseCors(CORS_POLICY);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
