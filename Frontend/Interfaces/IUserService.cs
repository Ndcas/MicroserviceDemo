using Frontend.Constants;
using Frontend.Dtos;

namespace Frontend.Interfaces;

internal interface IUserService
{
    Task<ApiResponse<AuthenticationData>> LoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default);

    Task<ApiResponse> LogoutAsync(CancellationToken cancellationToken = default);
}
