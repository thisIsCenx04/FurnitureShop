using FurnitureShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.ViewComponents;

public sealed class CategoryMenuViewComponent : ViewComponent
{
    private readonly FurnitureShopContext _db;

    public CategoryMenuViewComponent(FurnitureShopContext db) => _db = db;

    public sealed class Node
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int? ParentId { get; set; }
        public List<Node> Children { get; set; } = new();
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new Node
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentId = c.ParentId
            })
            .ToListAsync();

        // build tree
        var dict = categories.ToDictionary(x => x.Id);
        var roots = new List<Node>();

        foreach (var n in categories)
        {
            if (n.ParentId.HasValue && dict.TryGetValue(n.ParentId.Value, out var parent))
                parent.Children.Add(n);
            else
                roots.Add(n);
        }

        // sort children
        foreach (var r in roots)
            r.Children = r.Children.OrderBy(x => x.Name).ToList();

        return View(roots);
    }
}
