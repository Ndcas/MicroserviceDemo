using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;

namespace ProductService.Infrastructure.ChangeTracker;

internal class OutboxMessageTracker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxMessageTracker> _logger;
    private readonly TimeSpan _period = TimeSpan.FromSeconds(10);

    public OutboxMessageTracker(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OutboxMessageTracker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = _serviceScopeFactory.CreateAsyncScope();

                var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

                var response = await messageService.PublishUndeliveredMessagesAsync(stoppingToken);

                if (!response.Ok)
                {
                    _logger.LogWarning(response.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
