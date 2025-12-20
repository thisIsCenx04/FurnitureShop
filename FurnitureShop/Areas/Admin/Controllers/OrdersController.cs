using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using FurnitureShop.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace FurnitureShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class OrdersController : Controller
{
    private readonly FurnitureShopContext _db;
    private readonly NotificationService _noti;

    public OrdersController(FurnitureShopContext db, NotificationService noti)
    {
        _db = db;
        _noti = noti;
    }

    // GET: /Admin/Orders?status=0&from=...&to=...
    public async Task<IActionResult> Index(byte? status, DateTime? from, DateTime? to)
    {
        var q = _db.Orders.AsNoTracking();

        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (from.HasValue) q = q.Where(x => x.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(x => x.CreatedAt <= to.Value);

        var items = await q.OrderByDescending(x => x.CreatedAt).Take(500).ToListAsync();
        return View(items);
    }
    public async Task<IActionResult> Details(long id)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistories)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // Nếu bạn muốn lịch sử đã sort sẵn để view dùng:
        ViewBag.Histories = order.OrderStatusHistories
            .OrderByDescending(h => h.ChangedAt)
            .ToList();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(long id, byte toStatus, string? note)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        var fromStatus = order.Status;

        // validate transition (giữ rule của bạn)

        order.Status = toStatus;
        order.UpdatedAt = DateTime.UtcNow;

        var adminId = User.FindFirst("uid")?.Value; // hub của bạn cũng dùng uid
        _db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = adminId,
            Note = note,
            ChangedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // notify user nếu có UserId (Order có UserId) :contentReference[oaicite:8]{index=8}
        if (!string.IsNullOrWhiteSpace(order.UserId))
        {
            await _noti.NotifyUserAsync(
                order.UserId!,
                "order_status",
                "Cập nhật trạng thái đơn hàng",
                $"Đơn {order.OrderCode} đã chuyển trạng thái ({fromStatus} → {toStatus}).",
                $"/orders/{order.Id}"
            );
        }

        TempData["ok"] = "Đã cập nhật trạng thái.";
        return RedirectToAction(nameof(Details), new { id });
    }


    private static bool IsValidTransition(byte from, byte to)
    {
        // Status 0..4 :contentReference[oaicite:12]{index=12}
        return (from, to) switch
        {
            (0, 1) => true,
            (0, 4) => true,
            (1, 2) => true,
            (1, 4) => true,
            (2, 3) => true,
            _ => false
        };
    }
}
