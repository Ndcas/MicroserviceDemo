using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Broker;
using OrderService.Infrastructure.ChangeTracker;
using OrderService.Infrastructure.Constants;
using OrderService.Infrastructure.Database;
using OrderService.Infrastructure.Database.Repositories;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.DatabaseConnectionString);
        var pulsarConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.PulsarConnectionString);

        services.AddDbContext<OrderServiceContext>(option =>
            option.UseMySql(databaseConnectionString, ServerVersion.AutoDetect(databaseConnectionString)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMessageRepository, MessageRepository>();

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddSingleton<IPulsarClient>(sp => PulsarClient
            .Builder()
            .ServiceUrl(new Uri(pulsarConnectionString))
            .Build());

        services.AddScoped<ILogProducer, LogProducer>();

        services.AddScoped<IOrderCancellationProducer, OrderCancellationProducer>();

        services.AddScoped<IOrderCreationProducer, OrderCreationProducer>();

        services.AddScoped<IPaymentCompletionProducer, PaymentCompletionProducer>();

        services.AddHostedService<OutboxMessageTracker>();

        services.AddHostedService<CompletedReservationEventConsumer>();

        services.AddHostedService<FailedReservationEventConsumer>();

        return services;
    }
}
