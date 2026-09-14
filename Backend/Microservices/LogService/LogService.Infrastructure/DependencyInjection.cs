using DotPulsar;
using DotPulsar.Abstractions;
using LogService.Domain.Interfaces;
using LogService.Infrastructure.Broker;
using LogService.Infrastructure.Constants;
using LogService.Infrastructure.Database;
using LogService.Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LogService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.DatabaseConnectionString);
            var pulsarConnectionString = configuration.GetConnectionString(EnvironmentVariableKeys.PulsarConnectionString);

            services.AddDbContext<LogServiceContext>(option =>
                option.UseMySql(databaseConnectionString, ServerVersion.AutoDetect(databaseConnectionString)));

            services.AddSingleton<IPulsarClient>(sp => PulsarClient
                .Builder()
                .ServiceUrl(new Uri(pulsarConnectionString))
                .Build());

            services.AddScoped<ILogRepository, LogRepository>();

            services.AddHostedService<LogConsumer>();

            return services;
        }
    }
}
