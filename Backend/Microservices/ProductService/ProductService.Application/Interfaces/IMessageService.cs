using ProductService.Application.Dtos;

namespace ProductService.Application.Interfaces;

public interface IMessageService
{
    Task<ServiceResponse> PublishUndeliveredMessagesAsync(CancellationToken cancellationToken = default);
}
