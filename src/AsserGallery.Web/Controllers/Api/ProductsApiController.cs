using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsApiController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsApiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of clothing products with rich filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? subCategoryId,
        [FromQuery] int? colorId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] ProductStatus? status,
        [FromQuery] bool onlyInStock = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var result = await _mediator.Send(new GetProductsQuery(
            Search: search,
            CategoryId: categoryId,
            SubCategoryId: subCategoryId,
            ColorId: colorId,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            Status: status,
            OnlyInStock: onlyInStock,
            PageNumber: page,
            PageSize: pageSize
        ));

        return Ok(result);
    }

    /// <summary>
    /// Get detailed product information including variants and image sets
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        if (product == null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// Get available colors list with hex codes
    /// </summary>
    [HttpGet("colors")]
    public async Task<ActionResult<List<ColorDto>>> GetColors()
    {
        var colors = await _mediator.Send(new GetColorsQuery());
        return Ok(colors);
    }
}
