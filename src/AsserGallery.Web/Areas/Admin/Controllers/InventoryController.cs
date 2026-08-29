using AsserGallery.Application.Features.Categories.Queries;
using AsserGallery.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class InventoryController : Controller
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int? categoryId = null, string? search = null)
    {
        var products = await _mediator.Send(new GetProductsQuery(CategoryId: categoryId, Search: search, PageSize: 100));
        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: false));

        ViewBag.Categories = categories;
        ViewBag.CategoryId = categoryId;
        ViewBag.Search = search;

        return View(products.Items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(int? categoryId = null, string? search = null)
    {
        var result = await _mediator.Send(new ExportInventoryQuery(CategoryId: categoryId, Search: search));
        return File(result.Content, result.ContentType, result.FileName);
    }
}
