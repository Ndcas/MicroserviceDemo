using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Dtos;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers;

[Route("InternalProduct")]
[ApiController]
public class InternalProductController : ControllerBase
{
    private readonly IProductsService _productsService;
    private readonly ILogger<InternalProductController> _logger;

    public InternalProductController(IProductsService productsService, ILogger<InternalProductController> logger)
    {
        _productsService = productsService;
        _logger = logger;
    }

    [HttpGet("Price")]
    public async Task<IActionResult> GetPriceASync([FromQuery] IReadOnlyList<int> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _productsService.GetProductPriceAsync(ids, cancellationToken);

            if (!response.Ok)
            {
                _logger.LogWarning(response.Error);

                return StatusCode(response.Status, response.Error);
            }

            return StatusCode(response.Status, response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
