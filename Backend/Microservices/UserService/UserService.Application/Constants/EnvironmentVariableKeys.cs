namespace UserService.Application.Constants;

internal static class EnvironmentVariableKeys
{
    public const string JwtKey = "JWT:Key";
    public const string JwtIssuer = "JWT:Issuer";
    public const string JwtAudience = "JWT:Audience";
    public const string JwtAccessTokenExpirationMinutes = "JWT:AccessTokenExpireMinutes";
    public const string JwtRefreshTokenExpirationDays = "JWT:RefreshTokenExpireDays";
}

