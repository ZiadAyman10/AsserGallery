using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Categories.Queries;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Application.Features.Settings.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Controllers;

public class CatalogController : Controller
{
    private readonly IMediator _mediator;
    private readonly IWhatsAppLinkBuilder _whatsAppBuilder;

    public CatalogController(IMediator mediator, IWhatsAppLinkBuilder whatsAppBuilder)
    {
        _mediator = mediator;
        _whatsAppBuilder = whatsAppBuilder;
    }

    public async Task<IActionResult> Index(
        string? search = null,
        int? categoryId = null,
        int? subCategoryId = null,
        int? colorId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool onlyInStock = false,
        string? sortBy = null,
        int page = 1)
    {
        var settings = await _mediator.Send(new GetStoreSettingsQuery());
        if (settings.HideOutOfStock)
        {
            onlyInStock = true;
        }

        var query = new GetProductsQuery(
            Search: search,
            CategoryId: categoryId,
            SubCategoryId: subCategoryId,
            ColorId: colorId,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            OnlyInStock: onlyInStock,
            SortBy: sortBy,
            PageNumber: page,
            PageSize: 12
        );

        var products = await _mediator.Send(query);
        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: true));
        var colors = await _mediator.Send(new GetColorsQuery());

        ViewBag.Categories = categories;
        ViewBag.Colors = colors;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.SubCategoryId = subCategoryId;
        ViewBag.ColorId = colorId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        ViewBag.OnlyInStock = onlyInStock;
        ViewBag.SortBy = sortBy;
        ViewBag.Settings = settings;

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        if (product == null)
        {
            return NotFound();
        }

        var settings = await _mediator.Send(new GetStoreSettingsQuery());
        var isArabic = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("ar");
        var lang = isArabic ? "ar" : "en";

        var currentUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        var firstColor = product.Variants.FirstOrDefault(v => v.Quantity > 0);
        var colorName = isArabic ? firstColor?.ColorArabicName : firstColor?.ColorName;

        var whatsAppLink = _whatsAppBuilder.BuildOrderLink(
            phoneNumber: settings.WhatsAppNumber,
            productName: isArabic ? product.ArabicName : product.Name,
            colorName: colorName,
            price: product.DiscountedPrice ?? product.Price,
            productUrl: currentUrl,
            language: lang
        );

        ViewBag.Settings = settings;
        ViewBag.WhatsAppOrderLink = whatsAppLink;
        ViewBag.StoreWhatsApp = settings.WhatsAppNumber;
        ViewBag.MessengerUrl = $"https://m.me/{settings.MessengerUsername}";

        // Related products in the same subcategory
        var related = await _mediator.Send(new GetProductsQuery(SubCategoryId: product.SubCategoryId, PageSize: 4));
        ViewBag.RelatedProducts = related.Items.Where(p => p.Id != product.Id).Take(4).ToList();

        return View(product);
    }
}
