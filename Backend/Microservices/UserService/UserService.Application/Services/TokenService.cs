using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Constants;
using UserService.Application.Interfaces;

namespace UserService.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(int accountId, string username, int roleId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>(EnvironmentVariableKeys.JwtKey)));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration.GetValue<string>(EnvironmentVariableKeys.JwtIssuer);
        var audience = _configuration.GetValue<string>(EnvironmentVariableKeys.JwtAudience);
        var expirationMinutes = _configuration.GetValue<int>(EnvironmentVariableKeys.JwtAccessTokenExpirationMinutes);

        var claims = new Claim[]
        {
                new Claim(JwtConfigurations.ClaimTypeUserId, accountId.ToString()),
                new Claim(JwtConfigurations.ClaimTypeName, username),
                new Claim(JwtConfigurations.ClaimTypeJti, Guid.NewGuid().ToString()),
                new Claim(JwtConfigurations.ClaimTypeRoleId, roleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken(int accountId, string username, int roleId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>(EnvironmentVariableKeys.JwtKey)));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration.GetValue<string>(EnvironmentVariableKeys.JwtIssuer);
        var audience = _configuration.GetValue<string>(EnvironmentVariableKeys.JwtAudience);
        var expirationDays = _configuration.GetValue<int>(EnvironmentVariableKeys.JwtRefreshTokenExpirationDays);

        var claims = new Claim[]
        {
                new Claim(JwtConfigurations.ClaimTypeUserId, accountId.ToString()),
                new Claim(JwtConfigurations.ClaimTypeName, username),
                new Claim(JwtConfigurations.ClaimTypeJti, Guid.NewGuid().ToString()),
                new Claim(JwtConfigurations.ClaimTypeRoleId, roleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: credentials
        );

        var handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(token);
    }

    public async Task<IDictionary<string, object>?> ValidateRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>(EnvironmentVariableKeys.JwtKey)));

        var issuer = _configuration.GetValue<string>(EnvironmentVariableKeys.JwtIssuer);
        var audience = _configuration.GetValue<string>(EnvironmentVariableKeys.JwtAudience);

        var handler = new JwtSecurityTokenHandler();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var validationResult = await handler.ValidateTokenAsync(refreshToken, parameters);

        if (!validationResult.IsValid)
        {
            return null;
        }

        return validationResult.Claims;
    }
}
