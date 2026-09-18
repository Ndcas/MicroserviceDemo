using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UserService.Application.Constants;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;
using UserService.Application.Utils;
using UserService.Domain.Interfaces;

namespace UserService.Application.Services;

internal class AccountService : IAccountService
{
    private readonly IConfiguration _configuration;
    private readonly IAccountRepository _accountRepository;
    private readonly ICacheService _cache;
    private readonly ITokenService _token;

    public AccountService(
            IConfiguration configuration,
            IAccountRepository accountRepository,
            ICacheService cache,
            ITokenService token)
    {
        _configuration = configuration;
        _accountRepository = accountRepository;
        _cache = cache;
        _token = token;
    }

    public async Task<ServiceResponse<LoginResponseData>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (account is null)
        {
            return ServiceResponse<LoginResponseData>.Fail(StatusCodes.Status400BadRequest, AccountServiceMessages.IncorrectCredentials);
        }

        if (Hash.GenerateHash(request.Password) != account.Password)
        {
            return ServiceResponse<LoginResponseData>.Fail(StatusCodes.Status400BadRequest, AccountServiceMessages.IncorrectCredentials);
        }

        if (account.IsActive == AccountStatus.Disabled)
        {
            return ServiceResponse<LoginResponseData>.Fail(StatusCodes.Status400BadRequest, AccountServiceMessages.IncorrectCredentials);
        }

        var accessToken = _token.GenerateAccessToken(account.Id, account.Username, account.RoleId);
        var refreshToken = _token.GenerateRefreshToken(account.Id, account.Username, account.RoleId);

        var refreshTokenExpirationDays = _configuration.GetValue<int>(EnvironmentVariableKeys.JwtRefreshTokenExpirationDays);

        await _cache.SetAsync<string>(
            RedisKeys.RefreshToken(account.Id),
            refreshToken,
            TimeSpan.FromDays(refreshTokenExpirationDays),
            cancellationToken);

        var data = new LoginResponseData(accessToken, refreshToken);

        return ServiceResponse<LoginResponseData>.Success(StatusCodes.Status200OK, data);
    }

    public async Task<ServiceResponse> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var claims = await _token.ValidateRefreshTokenAsync(refreshToken, cancellationToken);

        if (claims is null)
        {
            return ServiceResponse.Fail(StatusCodes.Status400BadRequest, AccountServiceMessages.InvalidToken);
        }

        var accountId = int.Parse(claims[JwtConfigurations.ClaimTypeUserId].ToString());

        await _cache.RemoveAsync(RedisKeys.RefreshToken(accountId), cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse<RefreshResponseData>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var claims = await _token.ValidateRefreshTokenAsync(refreshToken, cancellationToken);

        if (claims is null)
        {
            return ServiceResponse<RefreshResponseData>.Fail(StatusCodes.Status400BadRequest, AccountServiceMessages.InvalidToken);
        }

        var accountId = int.Parse(claims[JwtConfigurations.ClaimTypeUserId].ToString());

        var cachedRefreshToken = await _cache.GetAsync<string>(RedisKeys.RefreshToken(accountId), cancellationToken);

        if (cachedRefreshToken is null || cachedRefreshToken != refreshToken)
        {
            return ServiceResponse<RefreshResponseData>.Fail(StatusCodes.Status400BadRequest, AccountServiceMessages.InvalidToken);
        }

        var username = claims[JwtConfigurations.ClaimTypeName].ToString();
        var roleId = int.Parse(claims[JwtConfigurations.ClaimTypeRoleId].ToString());

        var accessToken = _token.GenerateAccessToken(accountId, username, roleId);

        var refreshResponse = new RefreshResponseData(accessToken);

        return ServiceResponse<RefreshResponseData>.Success(StatusCodes.Status200OK, refreshResponse);
    }
}
