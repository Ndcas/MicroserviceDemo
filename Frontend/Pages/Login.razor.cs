using Frontend.Authentication;
using Frontend.Constants;
using Frontend.Dtos;
using Frontend.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Frontend.Pages;

public partial class Login
{
    [Inject]
    private IUserService UserService { get; set; } = default;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default;

    [Inject]
    private AuthenticationContext AuthenticationContext { get; set; } = default;

    [SupplyParameterFromForm(FormName = "loginForm")]
    public LoginCredentials Credentials { get; set; } = new LoginCredentials();

    public string Error { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await RedirectToSuitablePage();
    }

    public async Task PostLoginAsync()
    {
        Error = string.Empty;

        var response = await UserService.LoginAsync(Credentials);

        if (!response.Ok)
        {
            AuthenticationContext.ClearAuthentication();

            Error = response.Error;
        }
        else
        {
            await AuthenticationContext.Authenticate(response.Data.AccessToken);
        }

        await RedirectToSuitablePage();
    }

    public async Task RedirectToSuitablePage()
    {
        var authState = await AuthenticationContext.GetAuthenticationStateAsync();

        if (!authState.IsAuthenticated())
        {
            return;
        }

        var roleId = authState.GetRoleId();

        if (roleId == AccountRoles.Admin)
        {
            NavigationManager.NavigateTo(Routes.Orders);

            return;
        }

        if (roleId == AccountRoles.Buyer)
        {
            NavigationManager.NavigateTo(Routes.Product);

            return;
        }
    }
}
