using OrderService.Application.Dtos;

namespace OrderService.Application.Interfaces;

public interface IMessageService
{
    Task<ServiceResponse> PublishUndeliveredMessagesAsync(CancellationToken cancellationToken = default);
}
