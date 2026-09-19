using System.IdentityModel.Tokens.Jwt;
using Frontend;
using Frontend.Api;
using Frontend.Authentication;
using Frontend.Constants;
using Frontend.Interfaces;
using Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationContext>();

builder.Services.AddScoped(sp => (AuthenticationContext)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient(HttpClients.RefreshClientName, client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>(EnvironmentVariableKeys.BackendUrl));
});

builder.Services.AddHttpClient(HttpClients.BusinessClientName, client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>(EnvironmentVariableKeys.BackendUrl));
}).AddHttpMessageHandler<TokenHandler>();

builder.Services.AddLocalStorageServices();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<ICartService, CartService>();

await builder.Build().RunAsync();
