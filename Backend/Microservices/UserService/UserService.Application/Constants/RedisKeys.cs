namespace UserService.Application.Constants;

public static class RedisKeys
{
    public static string RefreshToken(int userId) => $"refresh_token_{userId}";
}
