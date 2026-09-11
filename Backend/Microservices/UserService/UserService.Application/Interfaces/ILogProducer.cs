using UserService.Application.Dtos;

namespace UserService.Application.Interfaces;

public interface ILogProducer : IAsyncDisposable
{
    Task SendAsync(LogMessage logMessage, CancellationToken cancellationToken = default);
}
