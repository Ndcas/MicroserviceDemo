using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Constants;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(HttpClientNames.ProductService, client =>
        {
            client.BaseAddress = new Uri(configuration[EnvironmentVariableKeys.ProductServiceApiUrl]);
        });

        services.AddScoped<IOrdersService, OrdersService>();

        services.AddScoped<IMessageService, MessageService>();

        return services;
    }
}
