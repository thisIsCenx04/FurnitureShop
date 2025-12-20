using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.Services.Cart;
using FurnitureShop.Services.Notifications;
using FurnitureShop.ViewModels.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers;

public sealed class CheckoutController : Controller
{
    private readonly FurnitureShopContext _db;
    private readonly ICartService _cart;
    private readonly NotificationService _noti;

    public CheckoutController(FurnitureShopContext db, ICartService cart, NotificationService noti)
    {
        _db = db;
        _cart = cart;
        _noti = noti;
    }

    private string? UserId => User.GetUserId();

    private async Task<(string? Name, string? Phone, string? Email, string? Address)> LoadProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId)) return (null, null, null, null);

        var u = await _db.Users
            .AsNoTracking()
            .Where(x => x.Id == UserId)
            .Select(x => new
            {
                Name = x.FullName,
                Phone = x.Phone,
                Email = x.Email,
                Address = x.Address
            })
            .FirstOrDefaultAsync();

        if (u == null) return (null, null, null, null);
        return (u.Name, u.Phone, u.Email, u.Address);
    }

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index()
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);
        var cart = await _cart.GetOrCreateCartAsync(UserId, cartKey);
        if (cart.Items.Count == 0) return Redirect("/cart");

        var (name, phone, email, address) = await LoadProfileAsync();

        var vm = new CheckoutVm
        {
            Cart = cart,
            CustomerName = name ?? "",
            Phone = phone ?? "",
            Email = email ?? "",
            Address = address ?? ""
        };

        return View(vm);
    }

    [HttpPost("/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutVm input)
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);
        var cart = await _cart.GetOrCreateCartAsync(UserId, cartKey);

        if (cart.Items.Count == 0) ModelState.AddModelError("", "Giỏ hàng trống.");

        if (string.IsNullOrWhiteSpace(input.CustomerName))
            ModelState.AddModelError(nameof(input.CustomerName), "Vui lòng nhập họ tên.");

        if (string.IsNullOrWhiteSpace(input.Phone))
            ModelState.AddModelError(nameof(input.Phone), "Vui lòng nhập số điện thoại.");

        if (string.IsNullOrWhiteSpace(input.Address))
            ModelState.AddModelError(nameof(input.Address), "Vui lòng nhập địa chỉ.");

        if (!ModelState.IsValid)
        {
            input.Cart = cart;
            return View("Index", input);
        }

        var productIds = cart.Items.Select(x => x.ProductId).Distinct().ToList();

        using var tx = await _db.Database.BeginTransactionAsync();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id);

        foreach (var ci in cart.Items)
        {
            if (!products.TryGetValue(ci.ProductId, out var p))
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Có sản phẩm không tồn tại hoặc đã ngừng bán.");
                input.Cart = cart;
                return View("Index", input);
            }

            if (ci.Quantity > p.Stock)
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("",
                    $"Sản phẩm '{p.Name}' chỉ còn {p.Stock} trong kho (bạn đặt {ci.Quantity}).");
                input.Cart = cart;
                return View("Index", input);
            }
        }

        decimal subtotal = 0m;
        decimal shipping = 0m;
        decimal discount = 0m;

        var order = new FurnitureShop.Models.Entities.Order
        {
            OrderCode = "OD" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            UserId = UserId,

            CustomerName = input.CustomerName.Trim(),
            Phone = input.Phone.Trim(),
            Email = input.Email?.Trim(),
            Address = input.Address.Trim(),
            Note = input.Note?.Trim(),

            PaymentMethod = 0,
            Status = 0,

            Subtotal = 0m,
            ShippingFee = shipping,
            DiscountAmount = discount,
            Total = 0m,

            CreatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var ci in cart.Items)
        {
            var p = products[ci.ProductId];

            var unit = (p.SalePrice ?? p.Price);
            var line = unit * ci.Quantity;

            subtotal += line;

            _db.OrderItems.Add(new FurnitureShop.Models.Entities.OrderItem
            {
                OrderId = order.Id,
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = unit,
                Quantity = ci.Quantity,
                LineTotal = line,
                CreatedAt = DateTime.UtcNow
            });

            p.Stock -= ci.Quantity;
            if (p.Stock < 0) p.Stock = 0;
        }

        order.Subtotal = subtotal;
        order.Total = subtotal + shipping - discount;

        await _db.SaveChangesAsync();

        await _cart.ClearCartAsync(cart.CartId);

        await tx.CommitAsync();

        await _noti.NotifyAdminsAsync(
            type: "new_order",
            title: "Có đơn hàng mới",
            message: $"Đơn {order.OrderCode} - {order.CustomerName} ({order.Total:n0})",
            url: $"/Admin/Orders/Details/{order.Id}"
        );

        return RedirectToAction(nameof(Success), new { id = order.Id });
    }

    [HttpGet("/checkout/success/{id:long}")]
    public async Task<IActionResult> Success(long id)
    {
        var order = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
}
