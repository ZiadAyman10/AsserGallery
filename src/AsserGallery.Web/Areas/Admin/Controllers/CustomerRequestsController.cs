using AsserGallery.Application.Features.CustomerRequests.Commands;
using AsserGallery.Application.Features.CustomerRequests.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CustomerRequestsController : Controller
{
    private readonly IMediator _mediator;

    public CustomerRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(CustomerRequestStatus? status = null)
    {
        var requests = await _mediator.Send(new GetCustomerRequestsQuery(status));
        ViewBag.SelectedStatus = status;
        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, CustomerRequestStatus status, string? adminNotes)
    {
        await _mediator.Send(new UpdateCustomerRequestStatusCommand(id, status, adminNotes));
        TempData["SuccessMessage"] = "Request status updated.";
        return RedirectToAction(nameof(Index));
    }
}
