using AsserGallery.Application.Features.Categories.Dtos;
using AsserGallery.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Controllers.Api;

[ApiController]
[Route("api/categories")]
[Produces("application/json")]
public class CategoriesApiController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesApiController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get category hierarchy and subcategories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] bool onlyActive = true)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(onlyActive));
        return Ok(result);
    }

    /// <summary>
    /// Get category by id with its subcategories
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }
}
