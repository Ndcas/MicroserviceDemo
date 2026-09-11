using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface IAccountService
{
    Task<ServiceResponse<LoginResponseData>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResponse<RefreshResponseData>> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<ServiceResponse> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
