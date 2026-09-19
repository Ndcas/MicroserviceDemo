using Frontend.Authentication;
using Frontend.Constants;
using Frontend.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Frontend.Layout;

public partial class MainLayout
{
    [Inject]
    private AuthenticationContext AuthenticationContext { get; set; } = default;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default;

    [Inject]
    private IUserService UserService { get; set; } = default;

    public async Task Logout()
    {
        AuthenticationContext.ClearAuthentication();

        try
        {
            await UserService.LogoutAsync();
        }
        finally
        {
            NavigationManager.NavigateTo(Routes.Login);
        }
    }
}
