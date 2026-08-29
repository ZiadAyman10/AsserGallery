using AsserGallery.Application.Features.Finances.Commands;
using AsserGallery.Application.Features.Finances.Queries;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FinancesController : Controller
{
    private readonly IMediator _mediator;

    public FinancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(TransactionType? type = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _mediator.Send(new GetFinancialTransactionsQuery(Type: type, Category: category, StartDate: startDate, EndDate: endDate));
        var summary = await _mediator.Send(new GetFinancialSummaryQuery());

        ViewBag.Summary = summary;
        ViewBag.SelectedType = type;
        ViewBag.SelectedCategory = category;

        return View(transactions);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var products = await _mediator.Send(new GetProductsQuery(PageSize: 100));
        ViewBag.Products = products.Items;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string title,
        string? description,
        decimal amount,
        TransactionType type,
        string category,
        DateTime? date,
        int? linkedProductId)
    {
        var command = new AddFinancialTransactionCommand(
            Title: title,
            Description: description,
            Amount: amount,
            Type: type,
            Category: category,
            Date: date ?? DateTime.UtcNow,
            LinkedProductId: linkedProductId
        );

        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Transaction '{title}' added successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteFinancialTransactionCommand(id));
        TempData["SuccessMessage"] = "Transaction removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(TransactionType? type = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var result = await _mediator.Send(new ExportFinancesQuery(Type: type, Category: category, StartDate: startDate, EndDate: endDate));
        return File(result.Content, result.ContentType, result.FileName);
    }
}
