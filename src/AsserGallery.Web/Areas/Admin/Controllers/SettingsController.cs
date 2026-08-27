using AsserGallery.Application.Features.Settings.Commands;
using AsserGallery.Application.Features.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await _mediator.Send(new GetStoreSettingsQuery());
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateStoreSettingsCommand command)
    {
        await _mediator.Send(command);
        TempData["SuccessMessage"] = "Store settings updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
