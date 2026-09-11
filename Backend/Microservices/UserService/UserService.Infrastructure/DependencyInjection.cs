using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using UserService.Application.Interfaces;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Broker;
using UserService.Infrastructure.Cache;
using UserService.Infrastructure.Constants;
using UserService.Infrastructure.Database;
using UserService.Infrastructure.Database.Repositories;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.DatabaseConnectionString);
        var pulsarConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.PulsarConnectionString);
        var redisConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.RedisConnectionString);

        services.AddDbContext<UserServiceContext>(option =>
            option.UseMySql(databaseConnectionString, ServerVersion.AutoDetect(databaseConnectionString)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAccountRepository, AccountRepository>();

        services.AddSingleton<IPulsarClient>(sp => PulsarClient.Builder().ServiceUrl(new Uri(pulsarConnectionString)).Build());

        services.AddSingleton<ILogProducer, LogProducer>();

        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}
