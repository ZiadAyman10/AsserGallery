using AsserGallery.Application.Features.CustomerRequests.Commands;
using AsserGallery.Application.Features.Settings.Queries;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Controllers;

public class ContactController : Controller
{
    private readonly IMediator _mediator;

    public ContactController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _mediator.Send(new GetStoreSettingsQuery());
        ViewBag.Settings = settings;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(
        string customerName,
        string phoneNumber,
        ContactChannel preferredChannel,
        string? message,
        int? productId,
        string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(phoneNumber))
        {
            TempData["ErrorMessage"] = "Please provide your name and phone number.";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        var command = new SubmitCustomerRequestCommand(
            CustomerName: customerName,
            PhoneNumber: phoneNumber,
            PreferredChannel: preferredChannel,
            Message: message,
            ProductId: productId
        );

        await _mediator.Send(command);

        var isArabic = System.Globalization.CultureInfo.CurrentUICulture.Name.StartsWith("ar");
        TempData["SuccessMessage"] = isArabic 
            ? "شكراً لتواصلك معنا! تم استلام طلبك وسيقوم فريقنا بالتواصل معك قريباً." 
            : "Thank you! Your request has been received, and our team will contact you shortly.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
