using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Controllers;

public sealed class ProductsController : Controller
{
    private readonly FurnitureShopContext _db;

    public ProductsController(FurnitureShopContext db) => _db = db;

    // GET: /products?category=slug&q=...&min=...&max=...&material=...&style=...&sort=...
    [HttpGet("/products")]
    public async Task<IActionResult> Index([FromQuery] ProductListFilterVm filter)
    {
        if (filter.Page <= 0) filter.Page = 1;
        if (filter.PageSize <= 0 || filter.PageSize > 60) filter.PageSize = 12;

        // Load categories (active) phục vụ:
        // - menu filter select
        // - resolve category slug -> id
        // - resolve descendant ids
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync();

        var nodeList = categories
    .Select(c => new CategoryNodeVm
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        ParentId = c.ParentId
    })
    .ToList();

        var dict = nodeList.ToDictionary(x => x.Id);
        var categoryTree = new List<CategoryNodeVm>();

        foreach (var n in nodeList)
        {
            if (n.ParentId.HasValue && dict.TryGetValue(n.ParentId.Value, out var parent))
                parent.Children.Add(n);
            else
                categoryTree.Add(n);
        }

        SortTree(categoryTree);

        var selectedSlugs = (filter.Categories ?? new List<string>())
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Select(x => x.Trim())
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

        if (selectedSlugs.Count == 0 && !string.IsNullOrWhiteSpace(filter.Category))
        {
            selectedSlugs.Add(filter.Category.Trim());
            // để UI multi vẫn hiện đúng
            filter.Categories = selectedSlugs;
        }

        // Lấy các category được chọn
        var selectedCats = selectedSlugs.Count == 0
            ? new List<Models.Entities.Category>()
            : categories.Where(c => selectedSlugs.Contains(c.Slug, StringComparer.OrdinalIgnoreCase)).ToList();

        if (selectedSlugs.Count > 0 && selectedCats.Count == 0)
        {
            return NotFound("Category not found.");
        }

        // Dùng cho UI hiển thị
        string? selectedCategoryName = selectedCats.Count == 1 ? selectedCats[0].Name : null;

        // Tính tập CategoryId hợp lệ: mỗi category đã chọn + toàn bộ con cháu
        HashSet<int>? selectedCategoryIds = null;

        if (selectedCats.Count > 0)
        {
            selectedCategoryIds = new HashSet<int>();
            foreach (var cat in selectedCats)
            {
                var ids = CategoryTreeHelper.GetDescendantIds(cat.Id, categories);
                foreach (var id in ids) selectedCategoryIds.Add(id);
            }
        }


        var flatCategories = CategoryTreeHelper.Flatten(categories);


        // Base query (join Categories để lấy tên/slug)
        var query =
            from p in _db.Products.AsNoTracking()
            join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id
            where p.IsActive && c.IsActive
            select new { p, c };

        // Filter theo nhiều category (bao gồm con cháu)
        if (selectedCategoryIds != null)
        {
            query = query.Where(x => selectedCategoryIds.Contains(x.p.CategoryId));
        }


        // Keyword search
        if (!string.IsNullOrWhiteSpace(filter.Q))
        {
            var q = filter.Q.Trim();
            query = query.Where(x =>
                x.p.Name.Contains(q) ||
                (x.p.ShortDescription != null && x.p.ShortDescription.Contains(q)) ||
                (x.p.Description != null && x.p.Description.Contains(q)));
        }

        // Material
        if (!string.IsNullOrWhiteSpace(filter.Material))
        {
            var m = filter.Material.Trim();
            query = query.Where(x => x.p.Material == m);
        }

        // Style
        if (!string.IsNullOrWhiteSpace(filter.Style))
        {
            var s = filter.Style.Trim();
            query = query.Where(x => x.p.Style == s);
        }

        // Price range: dùng giá hiệu lực = SalePrice ?? Price
        if (filter.Min.HasValue)
            query = query.Where(x => (x.p.SalePrice ?? x.p.Price) >= filter.Min.Value);

        if (filter.Max.HasValue)
            query = query.Where(x => (x.p.SalePrice ?? x.p.Price) <= filter.Max.Value);

        // Sorting
        query = filter.Sort switch
        {
            "price_asc" => query.OrderBy(x => (x.p.SalePrice ?? x.p.Price)),
            "price_desc" => query.OrderByDescending(x => (x.p.SalePrice ?? x.p.Price)),
            "name_asc" => query.OrderBy(x => x.p.Name),
            _ => query.OrderByDescending(x => x.p.CreatedAt) // newest
        };

        // Total count
        var total = await query.CountAsync();

        // Paging
        var skip = (filter.Page - 1) * filter.PageSize;

        var items = await query
            .Skip(skip)
            .Take(filter.PageSize)
            .Select(x => new ProductListItemVm
            {
                Id = x.p.Id,
                Name = x.p.Name,
                Slug = x.p.Slug,
                Price = x.p.Price,
                SalePrice = x.p.SalePrice,
                MainImageUrl = x.p.MainImageUrl,
                CategoryName = x.c.Name,
                CategorySlug = x.c.Slug
            })
            .ToListAsync();

        // Options for filter dropdowns (materials/styles)
        var materials = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Material != null && p.Material != "")
            .Select(p => p.Material!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var styles = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Style != null && p.Style != "")
            .Select(p => p.Style!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var vm = new ProductListVm
        {
            Filter = filter,
            Items = items,
            TotalItems = total,
            Categories = flatCategories,
            Materials = materials,
            Styles = styles,
            SelectedCategoryName = selectedCategoryName,
            CategoryTree = categoryTree
        };

        return View(vm);
    }

    // GET: /products/{slug}
    [HttpGet("/products/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        slug = (slug ?? "").Trim();
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        // Lấy product + category
        var data = await (
            from p in _db.Products.AsNoTracking()
            join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id
            where p.IsActive && c.IsActive && p.Slug == slug
            select new { p, c }
        ).FirstOrDefaultAsync();

        if (data == null) return NotFound("Product not found.");

        // Gallery từ ProductImages
        var images = await _db.ProductImages.AsNoTracking()
            .Where(i => i.ProductId == data.p.Id)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .ThenByDescending(i => i.CreatedAt)
            .Select(i => new ProductImageVm
            {
                Url = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder
            })
            .ToListAsync();

        var vm = new ProductDetailVm
        {
            Id = data.p.Id,
            Name = data.p.Name,
            Slug = data.p.Slug,
            CategoryName = data.c.Name,
            CategorySlug = data.c.Slug,

            ShortDescription = data.p.ShortDescription,
            Description = data.p.Description,
            Material = data.p.Material,
            Dimensions = data.p.Dimensions,
            Style = data.p.Style,

            Price = data.p.Price,
            SalePrice = data.p.SalePrice,
            Stock = data.p.Stock,

            MainImageUrl = data.p.MainImageUrl,
            Gallery = images
        };

        return View(vm);
    }

    static void SortTree(List<CategoryNodeVm> list)
    {
        list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        foreach (var n in list) SortTree(n.Children);
    }

}
