using OrderService.Application.Dtos;

namespace OrderService.Application.Interfaces;

public interface ILogProducer : IAsyncDisposable
{
    Task SendAsync(LogMessage logMessage, CancellationToken cancellationToken = default);
}
