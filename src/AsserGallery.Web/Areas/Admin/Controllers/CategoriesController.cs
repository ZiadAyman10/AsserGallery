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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, string arabicName, string? description, string? arabicDescription, int displayOrder)
    {
        var command = new CreateCategoryCommand(name, arabicName, description, arabicDescription, null, displayOrder);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Category '{name}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubCategory(int categoryId, string name, string arabicName, string? description, string? arabicDescription, int displayOrder)
    {
        var command = new CreateSubCategoryCommand(categoryId, name, arabicName, description, arabicDescription, displayOrder);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Subcategory '{name}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _mediator.Send(new DeleteCategoryCommand(id));
        TempData["SuccessMessage"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }
}
