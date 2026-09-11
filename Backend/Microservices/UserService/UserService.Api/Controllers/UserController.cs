using Microsoft.AspNetCore.Mvc;
using UserService.Api.Constants;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Api.Controllers;

[Route("/")]
[ApiController]
public class UserController : ControllerBase
{
    private const string _refreshTokenCookieName = "refreshToken";

    private readonly IAccountService _accountService;
    private readonly IConfiguration _configuration;
    private readonly ILogProducer _logProducer;


    public UserController(IAccountService accountService, IConfiguration configuration, ILogProducer logProducer)
    {
        _accountService = accountService;
        _configuration = configuration;
        _logProducer = logProducer;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromForm] LoginRequest requestData, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _accountService.LoginAsync(requestData, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(
                    Request,
                    $"Xử lý đăng nhập thất bại: {response.Error}",
                    Constants.LogLevel.Warning,
                    cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            int refreshTokenExpireDays = _configuration.GetValue<int>(EnvironmentVariableKeys.JwtRefreshTokenExpireDays);

            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(refreshTokenExpireDays)
            };

            Response.Cookies.Append(_refreshTokenCookieName, response.Data.RefreshToken, cookieOptions);

            await SendLogAsync(
                Request,
                "Xử lý đăng nhập thành công",
                Constants.LogLevel.Information,
                cancellationToken);

            return StatusCode(response.Status, new
            {
                accessToken = response.Data.AccessToken,
            });
        }
        catch (Exception ex)
        {
            await SendLogAsync(
                Request,
                $"Xử lý đăng nhập thất bại: {ex}",
                Constants.LogLevel.Error,
                cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken = Request.Cookies[_refreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Không tìm thấy refresh token");
            }

            var response = await _accountService.RefreshAsync(refreshToken, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(
                    Request,
                    $"Xử lý làm mới access token thất bại: {response.Error}",
                    Constants.LogLevel.Warning,
                    cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            await SendLogAsync(
                Request,
                "Xử lý làm mới access token thành công",
                Constants.LogLevel.Information,
                cancellationToken);

            return StatusCode(response.Status, new
            {
                accessToken = response.Data.AccessToken
            });
        }
        catch (Exception ex)
        {
            await SendLogAsync(
                Request,
                $"Xử lý làm mới access token thất bại: {ex}",
                Constants.LogLevel.Error,
                cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshToken = Request.Cookies[_refreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Không tìm thấy refresh token");
            }

            Response.Cookies.Delete(_refreshTokenCookieName);

            var response = await _accountService.LogoutAsync(refreshToken, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(
                    Request,
                    $"Xử lý đăng xuất thất bại: {response.Error}",
                    Constants.LogLevel.Warning,
                    cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            await SendLogAsync(
                Request,
                "Xử lý đăng xuất thành công",
                Constants.LogLevel.Information,
                cancellationToken);

            return StatusCode(StatusCodes.Status200OK);

        }
        catch (Exception ex)
        {
            await SendLogAsync(
                Request,
                $"Xử lý đăng xuất thất bại: {ex}",
                Constants.LogLevel.Error,
                cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private async Task SendLogAsync(
        HttpRequest request,
        string content,
        Constants.LogLevel level = Constants.LogLevel.Information,
        CancellationToken cancellationToken = default)
    {
        var correlationId = request.Headers[ProxyHeaders.CorrelationId];
        var ip = request.Headers[ProxyHeaders.Ip];

        var message = new LogMessage(
            nameof(level),
            "UserService-UserController",
            correlationId,
            ip,
            DateTime.Now.ToString(),
            content);

        await _logProducer.SendAsync(message, cancellationToken);
    }
}
