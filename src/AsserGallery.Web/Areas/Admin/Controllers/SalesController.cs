using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Application.Features.Sales.Commands;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Application.Features.Sales.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SalesController : Controller
{
    private readonly IMediator _mediator;

    public SalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(string? search = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = new GetSalesQuery(StartDate: startDate, EndDate: endDate, Search: search);
        var sales = await _mediator.Send(query);

        ViewBag.Search = search;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.TotalSalesAmount = sales.Sum(s => s.TotalAmount);

        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var products = await _mediator.Send(new GetProductsQuery(PageSize: 100, OnlyInStock: true));
        ViewBag.Products = products.Items;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string? customerName,
        string? customerPhone,
        string? notes,
        DateTime? saleDate,
        int[] productIds,
        int[] variantIds,
        int[] quantities,
        decimal[] unitPrices)
    {
        var items = new List<CreateSaleItemInput>();
        if (variantIds != null && quantities != null && unitPrices != null)
        {
            for (int i = 0; i < variantIds.Length; i++)
            {
                if (variantIds[i] > 0 && quantities[i] > 0)
                {
                    var prodId = (i < productIds.Length) ? productIds[i] : 0;
                    items.Add(new CreateSaleItemInput(prodId, variantIds[i], quantities[i], unitPrices[i]));
                }
            }
        }

        if (items.Count == 0)
        {
            TempData["ErrorMessage"] = "Please add at least one item to register a sale.";
            return RedirectToAction(nameof(Create));
        }

        try
        {
            var command = new RegisterSaleCommand(
                CustomerName: customerName,
                CustomerPhone: customerPhone,
                Notes: notes,
                SaleDate: saleDate ?? DateTime.UtcNow,
                Items: items
            );

            var saleId = await _mediator.Send(command);
            TempData["SuccessMessage"] = "Sale registered successfully! Inventory updated and income recorded.";
            return RedirectToAction(nameof(Details), new { id = saleId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to register sale: {ex.Message}";
            return RedirectToAction(nameof(Create));
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var sale = await _mediator.Send(new GetSaleByIdQuery(id));
        if (sale == null) return NotFound();

        return View(sale);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string? search = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var result = await _mediator.Send(new ExportSalesQuery(StartDate: startDate, EndDate: endDate, Search: search));
        return File(result.Content, result.ContentType, result.FileName);
    }
}
