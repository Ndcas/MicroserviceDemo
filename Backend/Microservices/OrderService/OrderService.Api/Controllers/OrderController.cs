using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Constants;
using OrderService.Application.Dtos;
using OrderService.Application.Interfaces;

namespace OrderService.Api.Controllers;

[Route("Order")]
[ApiController]
public class OrderController : ControllerBase
{
    private const string _logSource = "OrderService-OrderController";

    private readonly IOrdersService _ordersService;
    private readonly ILogProducer _logProducer;

    public OrderController(IOrdersService ordersService, ILogProducer logProducer)
    {
        _ordersService = ordersService;
        _logProducer = logProducer;
    }

    [HttpPost]
    [Authorize(Roles = AccountRoles.Buyer)]
    public async Task<IActionResult> PlaceOrderAsync(PlaceOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!int.TryParse(HttpContext.User.FindFirstValue(JwtConfigurations.ClaimTypeUserId), out var userId))
            {
                await SendLogAsync(OrderControllerMessages.CannotGetUserId, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var response = await _ordersService.PlaceOrderAsync(userId, request.Items, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(response.Error, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status, response.Data);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPatch("Cancel/{id:int}")]
    [Authorize(Roles = AccountRoles.Buyer)]
    public async Task<IActionResult> CancelOrderAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!int.TryParse(HttpContext.User.FindFirstValue(JwtConfigurations.ClaimTypeUserId), out var userId))
            {
                await SendLogAsync(OrderControllerMessages.CannotGetUserId, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var response = await _ordersService.CancelOrderAsync(userId, id, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(response.Error, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    //Demo only, replace by IPN webhook call later
    [HttpGet("Pay/{id:int}")]
    [Authorize(Roles = AccountRoles.Buyer)]
    public async Task<IActionResult> ConnfirmPaymentAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _ordersService.ConfirmPaymentAsync(id, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(response.Error, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPatch("Complete/{id:int}")]
    [Authorize(Roles = AccountRoles.Admin)]
    public async Task<IActionResult> CompleteOrdersync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _ordersService.CompleteOrderASync(id, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(response.Error, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = AccountRoles.Any)]
    public async Task<IActionResult> GetOrderDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = 0;
        var isBuyer = true;

        if (HttpContext.User.IsInRole(AccountRoles.Buyer))
        {
            if (!int.TryParse(HttpContext.User.FindFirstValue(JwtConfigurations.ClaimTypeUserId), out userId))
            {
                await SendLogAsync(OrderControllerMessages.CannotGetUserId, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }
        else if (HttpContext.User.IsInRole(AccountRoles.Admin))
        {
            isBuyer = false;
        }

        try
        {
            var response = isBuyer ?
                await _ordersService.GetOrderDetailsAsync(userId, id, cancellationToken) :
                await _ordersService.GetOrderDetailsAsync(id, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(response.Error, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status, response.Data);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [Authorize(Roles = AccountRoles.Any)]
    public async Task<IActionResult> GetOrdersAsync(
        [FromQuery] int page = DefaultPagination.Page,
        [FromQuery] int take = DefaultPagination.Take,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? DefaultPagination.Page : page;
        take = take < 1 || take > DefaultPagination.MaxTake ? DefaultPagination.Page : take;

        var userId = 0;
        var isBuyer = true;

        if (HttpContext.User.IsInRole(AccountRoles.Buyer))
        {
            if (!int.TryParse(HttpContext.User.FindFirstValue(JwtConfigurations.ClaimTypeUserId), out userId))
            {
                await SendLogAsync(OrderControllerMessages.CannotGetUserId, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }
        else if (HttpContext.User.IsInRole(AccountRoles.Admin))
        {
            isBuyer = false;
        }

        try
        {
            var response = isBuyer ?
                await _ordersService.GetOrdersAsync(page, take, userId, cancellationToken) :
                await _ordersService.GetOrdersAsync(page, take, cancellationToken);

            if (!response.Ok)
            {
                await SendLogAsync(response.Error, Constants.LogLevel.Warning, cancellationToken);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status, response.Data);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private async Task SendLogAsync(
        string content,
        Constants.LogLevel level = Constants.LogLevel.Information,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Request.Headers[ProxyHeaders.CorrelationId];
        var ip = Request.Headers[ProxyHeaders.Ip];

        var message = new LogMessage(
            nameof(level),
            _logSource,
            correlationId,
            ip,
            DateTime.Now.ToString(),
            content);

        await _logProducer.SendAsync(message, cancellationToken);
    }
}
