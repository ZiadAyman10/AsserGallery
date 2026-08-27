using AsserGallery.Application.Features.Dashboard.Queries;
using AsserGallery.Application.Features.Finances.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _mediator.Send(new GetDashboardSummaryQuery());
        var finances = await _mediator.Send(new GetFinancialSummaryQuery());

        ViewBag.Finances = finances;
        return View(dashboard);
    }
}
