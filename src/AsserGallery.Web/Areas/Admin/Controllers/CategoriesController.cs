using AsserGallery.Application.Features.Categories.Commands;
using AsserGallery.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(OnlyActive: false));
        return View(categories);
    }

    // ── Category Create ──────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, string arabicName, string? description, string? arabicDescription, int displayOrder)
    {
        var command = new CreateCategoryCommand(name, arabicName, description, arabicDescription, null, displayOrder);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Category '{name}' created.";
        return RedirectToAction(nameof(Index));
    }

    // ── Category Edit ────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, string name, string arabicName, string? description, string? arabicDescription, int displayOrder, bool isActive = true)
    {
        var command = new UpdateCategoryCommand(id, name, arabicName, description, arabicDescription, displayOrder, isActive);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Category '{name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    // ── Category Delete ──────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        TempData["SuccessMessage"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── SubCategory Create ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubCategory(int categoryId, string name, string arabicName, string? description, string? arabicDescription, int displayOrder)
    {
        var command = new CreateSubCategoryCommand(categoryId, name, arabicName, description, arabicDescription, displayOrder);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Subcategory '{name}' created.";
        return RedirectToAction(nameof(Index));
    }

    // ── SubCategory Edit ─────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubCategory(int id, string name, string arabicName, string? description, string? arabicDescription, int displayOrder, bool isActive = true)
    {
        var command = new UpdateSubCategoryCommand(id, name, arabicName, description, arabicDescription, displayOrder, isActive);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Subcategory '{name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    // ── SubCategory Delete ───────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubCategory(int id)
    {
        await _mediator.Send(new DeleteSubCategoryCommand(id));
        TempData["SuccessMessage"] = "Subcategory deleted.";
        return RedirectToAction(nameof(Index));
    }
}
