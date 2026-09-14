using Microsoft.AspNetCore.Mvc;
using ProductService.Api.Constants;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers;

[Route("/")]
[ApiController]
public class ProductController : ControllerBase
{
    private const string _logSource = "ProductService-ProductController";

    private readonly IProductsService _productsService;
    private readonly ILogProducer _logProducer;

    public ProductController(IProductsService productsService, ILogProducer logProducer)
    {
        _productsService = productsService;
        _logProducer = logProducer;
    }

    [HttpGet("Available")]
    public async Task<IActionResult> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _productsService.GetAvailableProductsAsync(cancellationToken);

            return StatusCode(response.Status, response.Data.Items);
        }
        catch (Exception ex)
        {
            await SendLogAsync(
                $"Xử lý lấy danh sách sản phẩm có sẵn thất bại: {ex}",
                Constants.LogLevel.Error,
                cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Cart")]
    public async Task<IActionResult> GetCartProductsAsync(
        [FromQuery] GetCartProductsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _productsService.GetCartProductsAsync(request, cancellationToken);

            return StatusCode(response.Status, response.Data.Items);
        }
        catch (Exception ex)
        {
            await SendLogAsync(
                $"Xử lý lấy danh sách sản phẩm trong giỏ hàng thất bại: {ex}",
                Constants.LogLevel.Error,
                cancellationToken);

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
