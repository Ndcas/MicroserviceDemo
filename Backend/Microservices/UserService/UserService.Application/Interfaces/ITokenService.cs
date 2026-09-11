namespace UserService.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(int accountId, string username, int roleId);

    string GenerateRefreshToken(int accountId, string username, int roleId);

    Task<IDictionary<string, object>?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
