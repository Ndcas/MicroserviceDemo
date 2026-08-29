using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace user.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(int accountId, string username, int roleId);

        string GenerateRefreshToken(int accountId, string username, int roleId);

        Task<IDictionary<string, object>?> ValidateRefreshToken(string refreshToken);
    }

    public class TokenService : ITokenService
    {
        private IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(int accountId, string username, int roleId)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT:Key")!));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            string issuer = _configuration.GetValue<string>("JWT:Issuer")!;
            string audience = _configuration.GetValue<string>("JWT:Audience")!;
            int expirationMinutes = _configuration.GetValue<int>("JWT:AccessTokenExpireMinutes");
            Claim[] claims = new Claim[]
            {
                new Claim("sub", accountId.ToString()),
                new Claim("name", username),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("role", roleId.ToString())
            };
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }

        public string GenerateRefreshToken(int accountId, string username, int roleId)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT:Key")!));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            string issuer = _configuration.GetValue<string>("JWT:Issuer")!;
            string audience = _configuration.GetValue<string>("JWT:Audience")!;
            int expirationDays = _configuration.GetValue<int>("JWT:RefreshTokenExpireDays");
            Claim[] claims = new Claim[]
            {
                new Claim("sub", accountId.ToString()),
                new Claim("name", username),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim("role", roleId.ToString())
            };
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expirationDays),
                signingCredentials: credentials
            );
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(token);
        }

        public async Task<IDictionary<string, object>?> ValidateRefreshToken(string refreshToken)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT:Key")!));
            string issuer = _configuration.GetValue<string>("JWT:Issuer")!;
            string audience = _configuration.GetValue<string>("JWT:Audience")!;
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters
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
            TokenValidationResult validationResult = await handler.ValidateTokenAsync(refreshToken, parameters);

            if (!validationResult.IsValid)
            {
                return null;
            }

            return validationResult.Claims;
        }
    }
}
