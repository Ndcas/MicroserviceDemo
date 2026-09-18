using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ProductService.Api.Constants;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers;

[Route("Product")]
[ApiController]
public class ProductController : ControllerBase
{
    private const string _logSource = "ProductService-ProductController";

    private readonly IProductsService _productsService;
    private readonly ILogProducer _logProducer;
    private readonly string _filePath;
    private readonly string _productFolder;
    private readonly string _brandFolder;

    public ProductController(IProductsService productsService, ILogProducer logProducer, IConfiguration configuration)
    {
        _productsService = productsService;
        _logProducer = logProducer;

        _filePath = configuration[EnvironmentVariableKeys.ImagePath];
        _productFolder = configuration[EnvironmentVariableKeys.ProductFolder];
        _brandFolder = configuration[EnvironmentVariableKeys.BrandFolder];
    }

    [HttpGet("Available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _productsService.GetAvailableProductsAsync(cancellationToken);

            return StatusCode(response.Status, response.Data);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Cart")]
    [Authorize(Roles = AccountRoles.Buyer)]
    public async Task<IActionResult> GetCartProductsAsync(
        [FromQuery] ProductIdsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _productsService.GetCartProductsAsync(request, cancellationToken);

            return StatusCode(response.Status, response.Data);
        }
        catch (Exception ex)
        {
            await SendLogAsync(ex.Message, Constants.LogLevel.Error, cancellationToken);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Image/Product/{name}")]
    public async Task<IActionResult> GetProductImageAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        name = Path.GetFileName(name);

        var fullPath = Path.Combine(_filePath, _productFolder, name);

        if (!System.IO.File.Exists(fullPath))
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fullPath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(fullPath, contentType);
    }

    [HttpGet("Image/Brand/{name}")]
    public async Task<IActionResult> GetBrandImageAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        name = Path.GetFileName(name);

        var fullPath = Path.Combine(_filePath, _brandFolder, name);

        if (!System.IO.File.Exists(fullPath))
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(fullPath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(fullPath, contentType);
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
