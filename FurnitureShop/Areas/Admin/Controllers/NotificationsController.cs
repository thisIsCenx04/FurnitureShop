using FurnitureShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class NotificationsController : Controller
{
    private readonly FurnitureShopContext _db;
    public NotificationsController(FurnitureShopContext db) => _db = db;

    // /Admin/Notifications
    public async Task<IActionResult> Index()
    {
        var items = await _db.Notifications.AsNoTracking()
            .Where(x => x.UserId == null) // admin broadcast
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(long id)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id);
        if (n != null)
        {
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
