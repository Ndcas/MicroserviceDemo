using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ProductService.Application.Constants;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Domain.Interfaces;

namespace ProductService.Application.Services;

internal class MessageService : IMessageService
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

            var eventMessage = JsonSerializer.Deserialize<OrderIdMessage>(message.Payload);

            if (message.Topic == _reservationCompletedTopic)
            {
                await _reservationEventProducer.NotifyReservationSucceededAsync(message.EventId, eventMessage, cancellationToken);

                continue;
            }

            if (message.Topic == _reservationFailedTopic)
            {
                await _reservationEventProducer.NotifyReservationFailedAsync(message.EventId, eventMessage, cancellationToken);

                continue;
            }
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }
}
