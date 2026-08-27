using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsserGallery.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ImageGuideController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
