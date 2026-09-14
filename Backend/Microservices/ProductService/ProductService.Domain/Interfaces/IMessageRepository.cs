using ProductService.Domain.Entities;

namespace ProductService.Domain.Interfaces;

public interface IMessageRepository
{
    void AddOutboxMessage(OutboxMessage message);

    void AddInboxMessage(InboxMessage message);

    Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default);

    Task<List<OutboxMessage>> GetUndeliveredMessagesAsync(CancellationToken cancellationToken = default);
}
