namespace UserService.Application.Constants;

internal static class RedisKeys
{
    public static string RefreshToken(int userId) => $"refresh_token_{userId}";
}
