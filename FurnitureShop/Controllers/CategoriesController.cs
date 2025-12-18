using FurnitureShop.Helpers;
using FurnitureShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers;

public sealed class CategoriesController : Controller
{
    private readonly FurnitureShopContext _db;

    public CategoriesController(FurnitureShopContext db) => _db = db;

    // GET: /categories
    [HttpGet("/categories")]
    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync();

        var flat = CategoryTreeHelper.Flatten(categories);
        return View(flat);
    }

    // GET: /categories/{slug} => redirect /products?category=slug
    [HttpGet("/categories/{slug}")]
    public IActionResult Detail(string slug)
    {
        slug = (slug ?? "").Trim();
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        return RedirectToAction("Index", "Products", new { area = "", category = slug });
    }
}
