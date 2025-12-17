using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.Areas.Admin.Controllers;

public sealed class DashboardController : AdminBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
