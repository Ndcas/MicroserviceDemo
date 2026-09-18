using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProductService.Application.Constants;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Application.Services;

internal class ProductsService : IProductsService
{
    private readonly IProductRepository _productRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _reservationCompletedTopic;
    private readonly string _reservationFailedTopic;

    public ProductsService(
        IConfiguration configuration,
        IProductRepository productRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;

        _reservationCompletedTopic = configuration[EnvironmentVariableKeys.ReservationCompletedTopic];
        _reservationFailedTopic = configuration[EnvironmentVariableKeys.ReseverationFailedTopic];
    }

    public async Task<ServiceResponse<IReadOnlyList<AvailableProductItem>>> GetAvailableProductsAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAvailableProductsAsync(cancellationToken);

        var responseData = products
            .Select(product => new AvailableProductItem(
                product.Id,
                product.ProductTypeId,
                product.BrandId,
                product.Name,
                product.Image,
                product.Price,
                product.Stocks - product.Reserved))
            .ToList();

        return ServiceResponse<IReadOnlyList<AvailableProductItem>>.Success(StatusCodes.Status200OK, responseData);
    }

    public async Task<ServiceResponse<IReadOnlyList<CartProductItem>>> GetCartProductsAsync(
        ProductIdsRequest request,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetProductsByIdsAsync(request.ProductIds, cancellationToken);

        var responseData = products
            .Select(product => new CartProductItem(
                product.Id,
                product.Name,
                product.Image,
                product.Price,
                product.Stocks - product.Reserved))
            .ToList();

        return ServiceResponse<IReadOnlyList<CartProductItem>>.Success(StatusCodes.Status200OK, responseData);
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

            return ServiceResponse.Success(StatusCodes.Status200OK);
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

            return ServiceResponse.Fail(StatusCodes.Status400BadRequest, ProductsServiceMessages.ItemCatalogMismatch);
        }

        var validStocks = true;

        var reservationEvent = new OrderIdMessage(message.OrderId);

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
                Topic = _reservationFailedTopic,
                Payload = JsonSerializer.Serialize(reservationEvent)
            });

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status400BadRequest, ProductsServiceMessages.ItemUnavailable);
        }

        foreach (var product in products)
        {
            product.Reserve(dictionary[product.Id].Quantity);
        }

        _messageRepository.AddOutboxMessage(new OutboxMessage()
        {
            EventId = Guid.NewGuid(),
            Topic = _reservationCompletedTopic,
            Payload = JsonSerializer.Serialize(reservationEvent)
        });

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status201Created);
    }

    public async Task<ServiceResponse> PerformStocksSubstractionAsync(
        Guid eventId,
        IReadOnlyList<ProductWithQuantityItem> message,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (await _messageRepository.IsProcessedAsync(eventId, cancellationToken))
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Success(StatusCodes.Status200OK);
        }

        _messageRepository.AddInboxMessage(new InboxMessage()
        {
            EventId = eventId
        });

        var dictionary = message.ToDictionary(item => item.ProductId);

        var products = await _productRepository.GetProductsByIdsForUpdateAsync(dictionary.Keys, cancellationToken);

        if (products.Count != message.Count)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status400BadRequest, ProductsServiceMessages.ItemCatalogMismatch);
        }

        foreach (var product in products)
        {
            product.SubstractPaidStocks(dictionary[product.Id].Quantity);
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse> PerformStocksUnreservationAsync(
        Guid eventId,
        IReadOnlyList<ProductWithQuantityItem> message,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (await _messageRepository.IsProcessedAsync(eventId, cancellationToken))
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Success(StatusCodes.Status200OK);
        }

        _messageRepository.AddInboxMessage(new InboxMessage()
        {
            EventId = eventId
        });

        var dictionary = message.ToDictionary(item => item.ProductId);

        var products = await _productRepository.GetProductsByIdsForUpdateAsync(dictionary.Keys, cancellationToken);

        if (products.Count != message.Count)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status400BadRequest, ProductsServiceMessages.ItemCatalogMismatch);
        }

        foreach (var product in products)
        {
            product.Unreserve(dictionary[product.Id].Quantity);
        }

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse<IReadOnlyList<ProductWithPriceItem>>> GetProductPriceAsync(
        IReadOnlyList<int> ids,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetProductsByIdsAsync(ids, cancellationToken);

        if (products.Count != ids.Count)
        {
            return ServiceResponse<IReadOnlyList<ProductWithPriceItem>>.Fail(
                StatusCodes.Status400BadRequest,
                ProductsServiceMessages.ItemCatalogMismatch);
        }

        var returnData = products
            .Select(product => new ProductWithPriceItem(product.Id, product.Price))
            .ToList();

        return ServiceResponse<IReadOnlyList<ProductWithPriceItem>>.Success(StatusCodes.Status200OK, returnData);
    }
}
