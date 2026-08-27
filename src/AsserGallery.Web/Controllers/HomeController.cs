using AsserGallery.Application.Features.Categories.Queries;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Application.Features.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var featuredProducts = await _mediator.Send(new GetProductsQuery(IsFeatured: true, PageSize: 8));
        var latestProducts = await _mediator.Send(new GetProductsQuery(PageSize: 8, SortBy: "date_desc"));
        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: true));
        var settings = await _mediator.Send(new GetStoreSettingsQuery());

        ViewBag.Categories = categories;
        ViewBag.LatestProducts = latestProducts.Items;
        ViewBag.Settings = settings;

        return View(featuredProducts.Items);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
