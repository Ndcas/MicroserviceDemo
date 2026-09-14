using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProductService.Application.Constants;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Application.Services;

public class ProductsService : IProductsService
{
    private readonly IConfiguration _configuration;
    private readonly IProductRepository _productRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsService(
        IConfiguration configuration,
        IProductRepository productRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _productRepository = productRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponse<GetAvailableProductsResponseData>> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAvailableProductsAsync(cancellationToken);

        var responseData = new GetAvailableProductsResponseData(products
            .Select(product => new AvailableProductItem(
                product.Id,
                product.ProductTypeId,
                product.BrandId,
                product.Name,
                product.Image,
                product.Price,
                product.Stocks - product.Reserved))
            .ToList());

        return new ServiceResponse<GetAvailableProductsResponseData>(
            true,
            StatusCodes.Status200OK,
            null,
            null,
            responseData);
    }

    public async Task<ServiceResponse<GetCartProductsResponseData>> GetCartProductsAsync(
        GetCartProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetProductsByIdsAsync(request.Ids, cancellationToken);

        var responseData = new GetCartProductsResponseData(products
            .Select(product => new CartProductItem(
                product.Id,
                product.Name,
                product.Image,
                product.Price,
                product.Stocks - product.Reserved))
            .ToList());

        return new ServiceResponse<GetCartProductsResponseData>(
            true,
            StatusCodes.Status200OK,
            null,
            null,
            responseData);
    }

    public async Task<ServiceResponse> PerformReservationAsync(
        Guid eventId,
        OrderCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (await _messageRepository.IsProcessedAsync(eventId, cancellationToken))
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return new ServiceResponse(
                true,
                StatusCodes.Status200OK,
                null,
                null);
        }

        _messageRepository.AddInboxMessage(new InboxMessage()
        {
            EventId = eventId
        });

        var dictionary = message.Items.ToDictionary(item => item.ProductId);

        var products = await _productRepository.GetProductsByIdsForUpdateAsync(dictionary.Keys, cancellationToken);

        if (products.Count != message.Items.Count)
        {
            await _unitOfWork.RollbackTransactionAsync();

            return new ServiceResponse(
                false,
                StatusCodes.Status400BadRequest,
                null,
                null);
        }

        var validStocks = true;

        var reservationEvent = new ReservationEventMessage(message.OrderId);

        foreach (var product in products)
        {
            if (!product.IsReservable(dictionary[product.Id].Quantity))
            {
                validStocks = false;

                break;
            }
        }

        if (!validStocks)
        {
            _messageRepository.AddOutboxMessage(new OutboxMessage()
            {
                EventId = Guid.NewGuid(),
                Topic = _configuration[EnvironmentVariableKeys.ReseverationFailedTopic],
                Payload = JsonSerializer.Serialize<ReservationEventMessage>(reservationEvent)
            });

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new ServiceResponse(
                false,
                StatusCodes.Status400BadRequest,
                null,
                null);
        }

        foreach (var product in products)
        {
            product.Reserve(dictionary[product.Id].Quantity);
        }

        _messageRepository.AddOutboxMessage(new OutboxMessage()
        {
            EventId = Guid.NewGuid(),
            Topic = _configuration[EnvironmentVariableKeys.ReservationCompletedTopic],
            Payload = JsonSerializer.Serialize<ReservationEventMessage>(reservationEvent)
        });

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ServiceResponse(
            true,
            StatusCodes.Status201Created,
            null,
            null);
    }

    public async Task<ServiceResponse> PerformStocksSubstractionAsync(
        Guid eventId,
        PaymentCompletedMessage message,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (await _messageRepository.IsProcessedAsync(eventId, cancellationToken))
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return new ServiceResponse(
                true,
                StatusCodes.Status200OK,
                null,
                null);
        }

        _messageRepository.AddInboxMessage(new InboxMessage()
        {
            EventId = eventId
        });

        var dictionary = message.Items.ToDictionary(item => item.ProductId);

        var products = await _productRepository.GetProductsByIdsForUpdateAsync(
            dictionary.Keys,
            cancellationToken);

        if (products.Count != message.Items.Count)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return new ServiceResponse(
                false,
                StatusCodes.Status400BadRequest,
                null,
                null);
        }

        foreach (var product in products)
        {
            product.SubstractPaidStocks(dictionary[product.Id].Quantity);
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return new ServiceResponse(
            true,
            StatusCodes.Status200OK,
            null,
            null);
    }
}
