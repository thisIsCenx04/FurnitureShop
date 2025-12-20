using FurnitureShop.Models;
using FurnitureShop.Models.Enums;
using FurnitureShop.ViewModels.Admin.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class UsersController : Controller
{
    private readonly FurnitureShopContext _db;
    public UsersController(FurnitureShopContext db) => _db = db;

    // GET: /Admin/Users?q=...
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string? q, [FromQuery] int page = 1)
    {
        if (page <= 0) page = 1;

        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(u =>
                u.UserName.Contains(q) ||
                u.Email.Contains(q));
        }

        query = query.OrderByDescending(u => u.CreatedAt);

        const int pageSize = 15;
        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserListVm.Item
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                Phone = u.Phone,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        var vm = new AdminUserListVm
        {
            Q = q,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };

        return View(vm);
    }

    // POST: /Admin/Users/ToggleActive
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return NotFound();

        // Optional: không cho tự khóa chính mình
        var myId = User.FindFirst("uid")?.Value;
        if (myId == user.Id) { TempData["Err"] = "Không thể khóa chính bạn."; return LocalRedirect(returnUrl ?? "/Admin/Users"); }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Ok"] = user.IsActive ? "Đã mở khóa user." : "Đã khóa user.";
        return LocalRedirect(returnUrl ?? "/Admin/Users");
    }

    // GET: /Admin/Users/Detail?id=...
    [HttpGet]
    public async Task<IActionResult> Detail(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest();

        var u = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (u == null) return NotFound();

        return View(u); // dùng trực tiếp entity User
    }

}
