using FurnitureShop.Helpers;
using FurnitureShop.Services.Cart;
using Microsoft.AspNetCore.Mvc;

namespace FurnitureShop.Controllers;

public sealed class CartController : Controller
{
    private readonly ICartService _cart;

    public CartController(ICartService cart) => _cart = cart;

    private string? UserId => User.GetUserId(); // nếu bạn đã có ClaimsExtensions
    // Nếu chưa có: bạn tự lấy Claim "uid"

    [HttpGet("/cart")]
    public async Task<IActionResult> Index()
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);
        var vm = await _cart.GetOrCreateCartAsync(UserId, cartKey);
        return View(vm);
    }

    [HttpPost("/cart/add")]
    public async Task<IActionResult> AddToCart(int productId, int qty = 1, string? returnUrl = null)
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);

        try
        {
            await _cart.AddToCartAsync(UserId, cartKey, productId, qty);
            TempData["CartMsg"] = "Đã thêm vào giỏ hàng.";
        }
        catch (Exception ex)
        {
            TempData["CartError"] = ex.Message;
        }

        return Redirect(returnUrl ?? "/cart");
    }


    [HttpPost("/cart/update")]
    public async Task<IActionResult> UpdateQty(long cartItemId, int qty)
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);

        await _cart.UpdateQtyAsync(UserId, cartKey, cartItemId, qty);
        TempData["CartMsg"] = "Đã cập nhật số lượng (tối đa theo tồn kho).";

        return Redirect("/cart");
    }


    [HttpPost("/cart/remove")]
    public async Task<IActionResult> RemoveItem(long cartItemId)
    {
        var cartKey = CartKeyHelper.GetOrCreateCartKey(HttpContext);
        await _cart.RemoveItemAsync(UserId, cartKey, cartItemId);
        return Redirect("/cart");
    }
}
