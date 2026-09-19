using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Broker;
using ProductService.Infrastructure.ChangeTracker;
using ProductService.Infrastructure.Constants;
using ProductService.Infrastructure.Database;
using ProductService.Infrastructure.Database.Repositories;

namespace ProductService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.DatabaseConnectionString);
        var pulsarConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.PulsarConnectionString);

        services.AddDbContext<ProductServiceContext>(option =>
            option.UseMySql(databaseConnectionString, ServerVersion.AutoDetect(databaseConnectionString)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IMessageRepository, MessageRepository>();

        services.AddSingleton<IPulsarClient>(sp => PulsarClient
            .Builder()
            .ServiceUrl(new Uri(pulsarConnectionString))
            .Build());

        services.AddScoped<ILogProducer, LogProducer>();

        services.AddScoped<IReservationEventProducer, ReservationEventProducer>();

        services.AddHostedService<OutboxMessageTracker>();

        services.AddHostedService<OrderCancellationConsumer>();

        services.AddHostedService<OrderCreationConsumer>();

        services.AddHostedService<PaymentCompletionConsumer>();

        return services;
    }
}
