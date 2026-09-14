using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Infrastructure.Database.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ProductServiceContext _context;

    public MessageRepository(ProductServiceContext context)
    {
        _context = context;
    }

    public void AddInboxMessage(InboxMessage message)
    {
        _context.Add(message);
    }

    public void AddOutboxMessage(OutboxMessage message)
    {
        _context.Add(message);
    }

    public async Task<List<OutboxMessage>> GetUndeliveredMessagesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.InboxMessages.AnyAsync(message => message.EventId == eventId, cancellationToken);
    }
}
