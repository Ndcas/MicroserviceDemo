using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OrderService.Application.Constants;
using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Services;

internal class OrdersService : IOrdersService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _productServiceHttpClient;
    private readonly string _orderCreatedTopic;
    private readonly string _orderCanceledTopic;
    private readonly string _paymentCompletedTopic;

    public OrdersService(
        IOrderRepository orderRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _orderRepository = orderRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;

        _productServiceHttpClient = httpClientFactory.CreateClient(HttpClientNames.ProductService);

        _orderCreatedTopic = configuration[EnvironmentVariableKeys.OrderCreatedTopic];
        _orderCanceledTopic = configuration[EnvironmentVariableKeys.OrderCreatedTopic];
        _paymentCompletedTopic = configuration[EnvironmentVariableKeys.PaymentCompletedTopic];
    }

    public async Task<ServiceResponse<PlaceOrderResponseData>> PlaceOrderAsync(
        int userId,
        IReadOnlyList<ProductWithQuantityItem> request,
        CancellationToken cancellationToken = default)
    {
        var url = $"{ProductServiceEndpoints.GetProductPrice}?{string.Join('&', request.Select(item => $"ids={item.ProductId}"))}";

        var productResponse = await _productServiceHttpClient.GetAsync(url, cancellationToken);

        if (!productResponse.IsSuccessStatusCode)
        {
            return ServiceResponse<PlaceOrderResponseData>.Fail(
                (int)productResponse.StatusCode,
                await productResponse.Content.ReadAsStringAsync(cancellationToken));
        }

        var dtoResponse = await productResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductWithPriceItem>>(cancellationToken);

        var priceDict = dtoResponse.ToDictionary(product => product.ProductId);

        var newOrder = new Order
        {
            UserId = userId
        };

        newOrder.AddDetails(request.Select(item => new OrderDetail
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            PriceAtBooking = priceDict[item.ProductId].Price
        }));

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        _orderRepository.AddOrder(newOrder);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = new OrderCreatedMessage(newOrder.Id, request);

        _messageRepository.AddOutboxMessage(new OutboxMessage()
        {
            EventId = Guid.NewGuid(),
            Topic = _orderCreatedTopic,
            Payload = JsonSerializer.Serialize(message)
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResponse<PlaceOrderResponseData>.Success(StatusCodes.Status201Created, new PlaceOrderResponseData(newOrder.Id));
    }

    public async Task<ServiceResponse> CancelOrderAsync(int userId, OrderIdMessage request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var order = await _orderRepository.GetOrderByIdForUpdateAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        if (order.UserId != userId)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status403Forbidden, OrderServiceMessages.InvalidPermisson);
        }

        order.Cancel();

        var message = order.OrderDetails
            .Select(detail => new ProductWithQuantityItem(detail.ProductId, detail.Quantity))
            .ToList();

        _messageRepository.AddOutboxMessage(new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            Topic = _orderCanceledTopic,
            Payload = JsonSerializer.Serialize(message)
        });

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse> CompleteOrderASync(OrderIdMessage request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var order = await _orderRepository.GetOrderByIdForUpdateAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        order.Finish();

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse> ConfirmReservationAsync(
        Guid eventId,
        OrderIdMessage request,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (await _messageRepository.IsProcessedAsync(eventId, cancellationToken))
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Success(StatusCodes.Status200OK);
        }

        _messageRepository.AddInboxMessage(new InboxMessage
        {
            EventId = eventId
        });

        var order = await _orderRepository.GetOrderByIdForUpdateAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        order.WaitForPayment();

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse> RemoveOrderAsync(
        Guid eventId,
        OrderIdMessage request,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (await _messageRepository.IsProcessedAsync(eventId, cancellationToken))
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Success(StatusCodes.Status200OK);
        }

        _messageRepository.AddInboxMessage(new InboxMessage
        {
            EventId = eventId
        });

        var order = await _orderRepository.GetOrderByIdForUpdateAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        await _orderRepository.RemoveOrderAsync(order, cancellationToken);

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse> ConfirmPaymentAsync(OrderIdMessage request, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var order = await _orderRepository.GetOrderWithDetailsByIdForUpdateAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return ServiceResponse.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        order.ConfirmPayment();

        var payload = order.OrderDetails
            .Select(detail => new ProductWithQuantityItem(detail.ProductId, detail.Quantity))
            .ToList();

        _messageRepository.AddOutboxMessage(new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            Topic = _paymentCompletedTopic,
            Payload = JsonSerializer.Serialize(payload)
        });

        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return ServiceResponse.Success(StatusCodes.Status200OK);
    }

    public async Task<ServiceResponse<OrderResponseData>> GetOrderDetailsAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResponse<OrderResponseData>.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        var data = new OrderResponseData(
            order.Id,
            order.UserId,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            order.OrderDetails
                .Select(detail => new OrderDetailItem(detail.ProductId, detail.PriceAtBooking, detail.Quantity))
                .ToList());

        return ServiceResponse<OrderResponseData>.Success(StatusCodes.Status200OK, data);
    }

    public async Task<ServiceResponse<OrderResponseData>> GetOrderDetailsAsync(
        int userId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderWithDetailsByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            return ServiceResponse<OrderResponseData>.Fail(StatusCodes.Status404NotFound, OrderServiceMessages.NotFound);
        }

        if (order.UserId != userId)
        {
            return ServiceResponse<OrderResponseData>.Fail(StatusCodes.Status403Forbidden, OrderServiceMessages.InvalidPermisson);
        }

        var data = new OrderResponseData(
            order.Id,
            order.UserId,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            order.OrderDetails
                .Select(detail => new OrderDetailItem(detail.ProductId, detail.PriceAtBooking, detail.Quantity))
                .ToList());

        return ServiceResponse<OrderResponseData>.Success(StatusCodes.Status200OK, data);
    }

    public async Task<ServiceResponse<IReadOnlyList<OrderResponseData>>> GetOrdersAsync(
        int page,
        int take,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrderListAsync(page, take, cancellationToken);

        var data = orders
            .Select(order => new OrderResponseData(
                order.Id,
                order.UserId,
                order.Status,
                order.CreatedAt,
                order.UpdatedAt,
                order.OrderDetails
                    .Select(detail => new OrderDetailItem(detail.ProductId, detail.PriceAtBooking, detail.Quantity))
                    .ToList()))
            .ToList();

        return ServiceResponse<IReadOnlyList<OrderResponseData>>.Success(StatusCodes.Status200OK, data);
    }

    public async Task<ServiceResponse<IReadOnlyList<OrderResponseData>>> GetOrdersAsync(
        int page,
        int take,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetOrderListAsync(userId, page, take, cancellationToken);

        var data = orders
            .Select(order => new OrderResponseData(
                order.Id,
                order.UserId,
                order.Status,
                order.CreatedAt,
                order.UpdatedAt,
                order.OrderDetails
                    .Select(detail => new OrderDetailItem(detail.ProductId, detail.PriceAtBooking, detail.Quantity))
                    .ToList()))
            .ToList();

        return ServiceResponse<IReadOnlyList<OrderResponseData>>.Success(StatusCodes.Status200OK, data);
    }
}
