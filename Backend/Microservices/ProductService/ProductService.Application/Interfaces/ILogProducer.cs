using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces;

public interface ILogProducer : IAsyncDisposable
{
    Task SendAsync(LogMessage logMessage, CancellationToken cancellationToken = default);
}
