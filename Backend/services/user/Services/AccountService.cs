using user.Controllers;
using user.Database;
using user.Dtos;
using user.Models;
using user.Utils;

namespace user.Services
{
    public interface IAccountService
    {
        Task<ServiceResponse<LogInResponse>> LogIn(LogInRequest request);

        Task<ServiceResponse<RefreshResponse>> Refresh(string refreshToken);

        Task<ServiceResponse> LogOut(string refreshToken);
    }

    public class AccountService : IAccountService
    {
        private IConfiguration _configuration;
        private IUnitOfWork _unitOfWork;
        private IAccountRepository _accountRepository;
        private ICacheService _cache;
        private ITokenService _token;

        public AccountService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ICacheService cache,
            ITokenService token
        )
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _cache = cache;
            _token = token;
        }

        public async Task<ServiceResponse<LogInResponse>> LogIn(LogInRequest request)
        {
            Account? account = await _accountRepository.GetAccountByUsername(request.Username);

            if (account == null)
            {
                return new ServiceResponse<LogInResponse>(false, 400, null, null, "Sai tên tài khoản hoặc mật khẩu");
            }

            if (Hash.GenerateHash(request.Password) != account.Password)
            {
                return new ServiceResponse<LogInResponse>(false, 400, null, null, "Sai tên tài khoản hoặc mật khẩu");
            }

            if (account.IsActive == "NO")
            {
                return new ServiceResponse<LogInResponse>(false, 400, null, null, "Tài khoản này đã bị vô hiệu hóa");
            }

            string accessToken = _token.GenerateAccessToken(account.Id, account.Username, account.RoleId);
            string refreshToken = _token.GenerateRefreshToken(account.Id, account.Username, account.RoleId);
            int refreshTokenExpirationDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpireDays");

            await _cache.Set<string>($"refresh_token_{account.Id}", refreshToken, TimeSpan.FromDays(refreshTokenExpirationDays));

            LogInResponse logInResponse = new LogInResponse(accessToken, refreshToken);

            return new ServiceResponse<LogInResponse>(true, 200, logInResponse, null, null);
        }

        public async Task<ServiceResponse<RefreshResponse>> Refresh(string refreshToken)
        {
            IDictionary<string, object>? claims = await _token.ValidateRefreshToken(refreshToken);

            if (claims == null)
            {
                return new ServiceResponse<RefreshResponse>(false, 400, null, null, "Refresh token không hợp lệ");
            }

            int accountId = int.Parse(claims["sub"].ToString()!);
            string? cachedRefreshToken = await _cache.Get<string>($"refresh_token_{accountId}");

            if (cachedRefreshToken == null || cachedRefreshToken != refreshToken)
            {
                return new ServiceResponse<RefreshResponse>(false, 400, null, null, "Refresh token không hợp lệ");
            }

            string username = claims["name"].ToString()!;
            int roleId = int.Parse(claims["role"].ToString()!);
            string accessToken = _token.GenerateAccessToken(accountId, username, roleId);
            RefreshResponse refreshResponse = new RefreshResponse(accessToken);

            return new ServiceResponse<RefreshResponse>(true, 200, refreshResponse, null, null);
        }

        public async Task<ServiceResponse> LogOut(string refreshToken)
        {
            IDictionary<string, object>? claims = await _token.ValidateRefreshToken(refreshToken);

            if (claims == null)
            {
                return new ServiceResponse(false, 400, null, "Refresh token không hợp lệ");
            }

            int accountId = int.Parse(claims["sub"].ToString()!);

            await _cache.Remove($"refresh_token_{accountId}");

            return new ServiceResponse(true, 200, "Đăng xuất thành công", null);
        }
    }
}
