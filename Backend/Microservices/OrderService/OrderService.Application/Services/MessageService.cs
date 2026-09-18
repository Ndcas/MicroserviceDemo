using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OrderService.Application.Constants;
using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Services;

internal class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderCancellationProducer _orderCancellationProducer;
    private readonly IOrderCreationProducer _orderCreationProducer;
    private readonly IPaymentCompletionProducer _paymentCompletionProducer;
    private readonly string _orderCanceledTopic;
    private readonly string _orderCreatedTopic;
    private readonly string _paymentCompletedTopic;

    public MessageService(
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IOrderCancellationProducer orderCancellationProducer,
        IOrderCreationProducer orderCreationProducer,
        IPaymentCompletionProducer paymentCompletionProducer,
        IConfiguration configuration)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _orderCancellationProducer = orderCancellationProducer;
        _orderCreationProducer = orderCreationProducer;
        _paymentCompletionProducer = paymentCompletionProducer;

        _orderCanceledTopic = configuration[EnvironmentVariableKeys.OrderCanceledTopic];
        _orderCreatedTopic = configuration[EnvironmentVariableKeys.OrderCreatedTopic];
        _paymentCompletedTopic = configuration[EnvironmentVariableKeys.PaymentCompletedTopic];
    }

    public async Task<ServiceResponse> PublishUndeliveredMessagesAsync(CancellationToken cancellationToken = default)
    {
        var messages = await _messageRepository.GetUndeliveredMessagesAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.UpdatePublishedTime();

            if (message.Topic == _orderCanceledTopic)
            {
                await _orderCancellationProducer.SendAsync(
                    message.EventId,
                    JsonSerializer.Deserialize<IReadOnlyList<ProductWithQuantityItem>>(message.Payload),
                    cancellationToken);

                continue;
            }

            if (message.Topic == _orderCreatedTopic)
            {
                await _orderCreationProducer.SendAsync(
                    message.EventId,
                    JsonSerializer.Deserialize<OrderCreatedMessage>(message.Payload),
                    cancellationToken);

                continue;
            }

            if (message.Topic == _paymentCompletedTopic)
            {
                await _paymentCompletionProducer.SendAsync(
                    message.EventId,
                    JsonSerializer.Deserialize<IReadOnlyList<ProductWithQuantityItem>>(message.Payload),
                    cancellationToken);

                continue;
            }
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }
}
