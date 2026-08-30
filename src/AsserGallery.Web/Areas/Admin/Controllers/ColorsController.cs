using AsserGallery.Application.Features.Colors.Commands;
using AsserGallery.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ColorsController : Controller
{
    private readonly IMediator _mediator;

    public ColorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var colors = await _mediator.Send(new GetColorsQuery());
        return View(colors);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string arabicName, string hexCode)
    {
        var command = new CreateColorCommand(name, arabicName, hexCode);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Color '{name}' added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string arabicName, string hexCode)
    {
        var command = new UpdateColorCommand(id, name, arabicName, hexCode);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Color '{name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteColorCommand(id));
        TempData["SuccessMessage"] = "Color deleted.";
        return RedirectToAction(nameof(Index));
    }
}
