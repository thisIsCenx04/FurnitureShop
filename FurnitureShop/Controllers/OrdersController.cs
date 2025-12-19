using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers;

public sealed class OrdersController : Controller
{
    private readonly FurnitureShopContext _db;
    public OrdersController(FurnitureShopContext db) => _db = db;

    private string? UserId => User.GetUserId();

    [Authorize]
    [HttpGet("/orders")]
    public async Task<IActionResult> My()
    {
        var uid = UserId;
        if (string.IsNullOrEmpty(uid)) return Challenge();

        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.UserId == uid)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new MyOrdersItemVm
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                Total = o.Total
            })
            .ToListAsync();

        return View("My", orders);
    }

    [Authorize]
    [HttpGet("/orders/{id:long}")]
    public async Task<IActionResult> Detail(long id)
    {
        var uid = UserId;
        if (string.IsNullOrEmpty(uid)) return Challenge();

        var order = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == uid);

        if (order == null) return NotFound();

        var vm = await BuildDetailVm(order);
        return View("Detail", vm);
    }

    [HttpGet("/orders/track")]
    public IActionResult Track() => View();

    [HttpPost("/orders/track")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Track(string orderCode, string phone)
    {
        orderCode = (orderCode ?? "").Trim();
        phone = (phone ?? "").Trim();

        if (string.IsNullOrWhiteSpace(orderCode) || string.IsNullOrWhiteSpace(phone))
        {
            ViewBag.Error = "Vui lòng nhập mã đơn và số điện thoại.";
            return View();
        }

        var order = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.Phone == phone);

        if (order == null)
        {
            ViewBag.Error = "Không tìm thấy đơn hàng (kiểm tra lại mã đơn và SĐT).";
            return View();
        }

        var vm = await BuildDetailVm(order);
        return View("Detail", vm);
    }

    private async Task<OrderDetailVm> BuildDetailVm(Models.Entities.Order order)
    {
        var items = await _db.OrderItems.AsNoTracking()
            .Where(i => i.OrderId == order.Id)
            .OrderBy(i => i.Id)
            .Select(i => new OrderItemVm
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            })
            .ToListAsync();

        var timeline = await _db.OrderStatusHistories.AsNoTracking()
            .Where(h => h.OrderId == order.Id)
            .OrderBy(h => h.ChangedAt)
            .Select(h => new OrderStatusHistoryVm
            {
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                Note = h.Note,
                ChangedAt = h.ChangedAt
            })
            .ToListAsync();

        if (timeline.Count == 0)
        {
            timeline.Add(new OrderStatusHistoryVm
            {
                FromStatus = order.Status,
                ToStatus = order.Status,
                Note = "Tạo đơn hàng",
                ChangedAt = order.CreatedAt
            });
        }

        return new OrderDetailVm
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            CreatedAt = order.CreatedAt,
            CustomerName = order.CustomerName,
            Phone = order.Phone,
            Email = order.Email,
            Address = order.Address,

            Status = order.Status,
            PaymentMethod = order.PaymentMethod,

            Subtotal = order.Subtotal,
            ShippingFee = order.ShippingFee,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,

            Items = items,
            Timeline = timeline
        };
    }
}
