using FurnitureShop.Helpers;
using FurnitureShop.Services.Cart;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.Controllers;

public sealed class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    [HttpGet("/cart")]
    public async Task<IActionResult> Index()
    {
        var cartKey = CartKeyHelper.GetOrSetCartKey(HttpContext);
        var userId = User.GetUserIdOrNull();

        var vm = await _cartService.GetOrCreateCartAsync(userId, cartKey);
        return View(vm);
    }

    [HttpPost("/cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int qty = 1)
    {
        var cartKey = CartKeyHelper.GetOrSetCartKey(HttpContext);
        var userId = User.GetUserIdOrNull();

        try
        {
            await _cartService.AddToCartAsync(userId, cartKey, productId, qty);
            TempData["Toast"] = "Đã thêm vào giỏ hàng.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ToastError"] = ex.Message;
        }

        return Redirect("/cart");
    }

    [HttpPost("/cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long cartItemId, int qty)
    {
        var cartKey = CartKeyHelper.GetOrSetCartKey(HttpContext);
        var userId = User.GetUserIdOrNull();

        await _cartService.UpdateQtyAsync(userId, cartKey, cartItemId, qty);
        TempData["Toast"] = "Đã cập nhật số lượng (tối đa theo tồn kho).";

        return Redirect("/cart");
    }

    [HttpPost("/cart/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(long cartItemId)
    {
        var cartKey = CartKeyHelper.GetOrSetCartKey(HttpContext);
        var userId = User.GetUserIdOrNull();

        await _cartService.RemoveItemAsync(userId, cartKey, cartItemId);
        TempData["Toast"] = "Đã xóa sản phẩm khỏi giỏ.";

        return Redirect("/cart");
    }
}
