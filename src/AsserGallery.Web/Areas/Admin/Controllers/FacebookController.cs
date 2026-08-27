using AsserGallery.Application.Features.Facebook.Commands;
using AsserGallery.Application.Features.Facebook.Queries;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class FacebookController : Controller
{
    private readonly IMediator _mediator;

    public FacebookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var destinations = await _mediator.Send(new GetFacebookDestinationsQuery());
        var products = await _mediator.Send(new GetProductsQuery(PageSize: 100));

        ViewBag.Products = products.Items;
        return View(destinations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDestination(string name, DestinationType destinationType, string targetIdOrUrl, string? accessToken)
    {
        var command = new CreateFacebookDestinationCommand(name, destinationType, targetIdOrUrl, accessToken);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = $"Facebook destination '{name}' added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GroupAssistant(int? productId = null, int? destinationId = null, string lang = "ar")
    {
        var destinations = await _mediator.Send(new GetFacebookDestinationsQuery());
        var groupDestinations = destinations.Where(d => d.DestinationType == DestinationType.Group).ToList();
        var products = await _mediator.Send(new GetProductsQuery(PageSize: 100));

        ViewBag.Destinations = groupDestinations;
        ViewBag.Products = products.Items;
        ViewBag.SelectedProductId = productId;
        ViewBag.SelectedDestinationId = destinationId;
        ViewBag.Language = lang;

        if (productId.HasValue && destinationId.HasValue)
        {
            var generated = await _mediator.Send(new GenerateGroupPostQuery(productId.Value, destinationId.Value, lang));
            ViewBag.GeneratedPost = generated;
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmGroupPost(int productId, int destinationId, string postContent, string? postUrl, string? notes)
    {
        var command = new LogGroupPostConfirmationCommand(productId, destinationId, postContent, postUrl, notes);
        await _mediator.Send(command);
        TempData["SuccessMessage"] = "Group post confirmed and recorded in posting log.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishPage(int productId, int destinationId, string message)
    {
        var command = new PublishToFacebookPageCommand(productId, destinationId, message);
        var result = await _mediator.Send(command);

        if (result.Success)
        {
            TempData["SuccessMessage"] = $"Post published successfully to Facebook Page! (Post ID: {result.PostId})";
        }
        else
        {
            TempData["ErrorMessage"] = $"Failed to publish: {result.ErrorMessage}";
        }

        return RedirectToAction(nameof(Index));
    }
}
