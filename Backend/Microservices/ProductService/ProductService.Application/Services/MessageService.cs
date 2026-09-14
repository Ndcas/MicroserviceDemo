using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Domain.Interfaces;
using ProductService.Application.Constants;
using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservationEventProducer _reservationEventProducer;
    private readonly string _reservationCompletedTopic;
    private readonly string _reservationFailedTopic;

    public MessageService(
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IReservationEventProducer reservationEventProducer,
        IConfiguration configuration)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _reservationEventProducer = reservationEventProducer;
        _reservationCompletedTopic = configuration[EnvironmentVariableKeys.ReservationCompletedTopic];
        _reservationFailedTopic = configuration[EnvironmentVariableKeys.ReseverationFailedTopic];
    }

    public async Task<ServiceResponse> PublishUndeliveredMessagesAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _messageRepository.GetUndeliveredMessagesAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.UpdatePublishedTime();

            var eventMessage = JsonSerializer.Deserialize<ReservationEventMessage>(message.Payload);

            if (message.Topic == _reservationCompletedTopic)
            {
                await _reservationEventProducer.NotifyReservationSucceededAsync(eventMessage, cancellationToken);

                continue;
            }

            if (message.Topic == _reservationFailedTopic)
            {
                await _reservationEventProducer.NotifyReservationFailedAsync(eventMessage, cancellationToken);

                continue;
            }
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ServiceResponse(
            true,
            StatusCodes.Status200OK,
            null,
            null);
    }
}
