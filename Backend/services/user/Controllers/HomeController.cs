using Microsoft.AspNetCore.Mvc;
using user.Dtos;
using user.Pulsar;
using user.Services;

namespace user.Controllers
{
    [Route("/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private IAccountService _account;
        private IConfiguration _configuration;
        private ILogProducer _log;

        public HomeController(IAccountService account, IConfiguration configuration, ILogProducer log)
        {
            _account = account;
            _configuration = configuration;
            _log = log;
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn(
            [FromHeader(Name = "X-Correlation-Id")] string correlationId,
            [FromHeader(Name = "X-Ip")] string ip,
            [FromForm] LogInRequest requestData
        )
        {
            try
            {
                await _log.Send(correlationId, ip, "Bắt đầu xử lý đăng nhập");

                ServiceResponse<LogInResponse> response = await _account.LogIn(requestData);

                if (!response.Ok)
                {
                    await _log.Send(correlationId, ip, $"Xử lý đăng nhập thất bại: {response.Error}", LogLevel.Warning);

                    return StatusCode(response.Status, response.Error);
                }

                int refreshTokenExpireDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpireDays");
                CookieOptions cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(refreshTokenExpireDays)
                };

                Response.Cookies.Append("refreshToken", response.Data.RefreshToken, cookieOptions);

                await _log.Send(correlationId, ip, "Xử lý đăng nhập thành công");

                return Ok(new { accessToken = response.Data.AccessToken });
            }
            catch (Exception e)
            {
                await _log.Send(correlationId, ip, $"Xử lý đăng nhập thất bại: {e}", LogLevel.Error);

                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh(
            [FromHeader(Name = "X-Correlation-Id")] string correlationId,
            [FromHeader(Name = "X-Ip")] string ip
        )
        {
            try
            {
                await _log.Send(correlationId, ip, "Bắt đầu xử lý làm mới access token");

                string? refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return StatusCode(401, "Không tìm thấy refresh token");
                }

                ServiceResponse<RefreshResponse> response = await _account.Refresh(refreshToken);

                if (!response.Ok)
                {
                    await _log.Send(correlationId, ip, $"Xử lý làm mới access token thất bại: {response.Error}", LogLevel.Warning);

                    return StatusCode(response.Status, response.Error);
                }

                return Ok(new { accessToken = response.Data.AccessToken });
            }
            catch (Exception e)
            {
                await _log.Send(correlationId, ip, $"Xử lý làm mới access token thất bại: {e}", LogLevel.Error);

                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        [HttpPost("LogOut")]
        public async Task<IActionResult> LogOut(
            [FromHeader(Name = "X-Correlation-Id")] string correlationId,
            [FromHeader(Name = "X-Ip")] string ip
        )
        {
            try
            {
                await _log.Send(correlationId, ip, "Bắt đầu xử lý đăng xuất");

                string? refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return StatusCode(401, "Không tìm thấy refresh token");
                }

                ServiceResponse response = await _account.LogOut(refreshToken);

                Response.Cookies.Delete("refreshToken");

                if (!response.Ok)
                {
                    await _log.Send(correlationId, ip, $"Xử lý đăng xuất thất bại: {response.Error}", LogLevel.Warning);

                    return StatusCode(response.Status, response.Error);
                }

                return Ok("Đăng xuất thành công");
            }
            catch (Exception e)
            {
                await _log.Send(correlationId, ip, $"Xử lý đăng xuất thất bại: {e}", LogLevel.Error);

                return StatusCode(500, "Lỗi hệ thống");
            }
        }
    }
}
