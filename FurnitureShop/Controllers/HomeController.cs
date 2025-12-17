using System.Diagnostics;
using FurnitureShop.Models;
using FurnitureShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers;

public sealed class HomeController : Controller
{
    private readonly FurnitureShopContext _db;

    public HomeController(FurnitureShopContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Lấy nhanh vài danh mục + sản phẩm demo
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.ParentId == null)
            .OrderBy(c => c.Name)
            .Take(6)
            .ToListAsync();

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        var vm = new HomeIndexVm
        {
            Categories = categories,
            Products = products
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
