using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.Services.Cart;
using FurnitureShop.ViewModels.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers;

public sealed class CheckoutController : Controller
{
    private readonly FurnitureShopContext _db;
    private readonly ICartService _cart;

    public CheckoutController(FurnitureShopContext db, ICartService cart)
    {
        _db = db;
        _cart = cart;
    }

    private string? UserId => User.GetUserId();

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index()
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);
        var cart = await _cart.GetOrCreateCartAsync(UserId, cartKey);
        if (cart.Items.Count == 0) return Redirect("/cart");
        return View(new CheckoutVm { Cart = cart });
    }

    [HttpPost("/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutVm input)
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);
        var cart = await _cart.GetOrCreateCartAsync(UserId, cartKey);

        if (cart.Items.Count == 0) ModelState.AddModelError("", "Giỏ hàng trống.");
        if (!ModelState.IsValid)
        {
            input.Cart = cart;
            return View("Index", input);
        }

        // Load Products WITH TRACKING để update Stock
        var productIds = cart.Items.Select(x => x.ProductId).Distinct().ToList();

        using var tx = await _db.Database.BeginTransactionAsync();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id);

        // 1) CHECK TỒN KHO: cart qty <= stock
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

        var order = new Models.Entities.Order
        {
            OrderCode = "OD" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            UserId = UserId,

            CustomerName = input.CustomerName.Trim(),
            Phone = input.Phone.Trim(),
            Email = input.Email?.Trim(),
            Address = input.Address.Trim(),
            Note = input.Note?.Trim(),

            PaymentMethod = 0, // COD
            Status = 0,

            Subtotal = 0m,
            ShippingFee = shipping,
            DiscountAmount = discount,
            Total = 0m,

            CreatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // 3) TRỪ TỒN KHO + tạo OrderItems snapshot
        foreach (var ci in cart.Items)
        {
            var p = products[ci.ProductId];

            var unit = (p.SalePrice ?? p.Price);
            var line = unit * ci.Quantity;

            subtotal += line;

            _db.OrderItems.Add(new Models.Entities.OrderItem
            {
                OrderId = order.Id,
                ProductId = p.Id,
                ProductName = p.Name,
                UnitPrice = unit,
                Quantity = ci.Quantity,
                LineTotal = line,
                CreatedAt = DateTime.UtcNow
            });

            // trừ tồn kho
            p.Stock -= ci.Quantity;
            if (p.Stock < 0) p.Stock = 0; // safety
        }

        order.Subtotal = subtotal;
        order.Total = subtotal + shipping - discount;

        await _db.SaveChangesAsync();

        // clear cart
        await _cart.ClearCartAsync(cart.CartId);

        await tx.CommitAsync();

        // 2) màn hình thông báo đã đặt hàng
        return RedirectToAction(nameof(Success), new { id = order.Id });
    }

    // 2) Success screen
    [HttpGet("/checkout/success/{id:long}")]
    public async Task<IActionResult> Success(long id)
    {
        var order = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
}
