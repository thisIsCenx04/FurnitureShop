using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using FurnitureShop.ViewModels.Admin.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Areas.Admin.Controllers;

[Area("Admin")]
// [Authorize(Policy = "AdminOnly")]
public sealed class CategoriesController : Controller
{
    private readonly FurnitureShopContext _db;

    public CategoriesController(FurnitureShopContext db) => _db = db;

    // GET: /Admin/Categories
    public async Task<IActionResult> Index()
    {
        var rows = await (
            from c in _db.Categories.AsNoTracking()
            join p in _db.Categories.AsNoTracking() on c.ParentId equals p.Id into gj
            from parent in gj.DefaultIfEmpty()
            orderby c.Name
            select new CategoryRowVm
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentName = parent != null ? parent.Name : null,
                IsActive = c.IsActive
            }
        ).ToListAsync();

        return View(rows);
    }

    // GET: /Admin/Categories/Create
    public async Task<IActionResult> Create()
    {
        var vm = new CategoryFormVm();
        await LoadParentOptions(vm, excludeId: null);
        return View(vm);
    }

    // POST: /Admin/Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormVm vm)
    {
        vm.Slug = (vm.Slug ?? "").Trim();
        vm.Name = (vm.Name ?? "").Trim();

        vm.Name = (vm.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Tên không được để trống.");

        vm.Slug = await SlugHelper.GenerateUniqueSlugAsync(
            _db.Categories,
            vm.Name,
            x => x.Slug,
            x => x.Id
        );

        // parent cannot be itself (create: không có Id)
        // parent cycle: create chưa thể có cycle, nhưng vẫn check ParentId tồn tại
        if (vm.ParentId.HasValue)
        {
            var parentExists = await _db.Categories.AsNoTracking().AnyAsync(x => x.Id == vm.ParentId.Value);
            if (!parentExists)
                ModelState.AddModelError(nameof(vm.ParentId), "Danh mục cha không hợp lệ.");
        }

        if (!ModelState.IsValid)
        {
            await LoadParentOptions(vm, excludeId: null);
            return View(vm);
        }

        var entity = new Category
        {
            Name = vm.Name,
            Slug = vm.Slug,
            Description = vm.Description,
            ParentId = vm.ParentId,
            IsActive = vm.IsActive
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Đã tạo danh mục.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    // GET: /Admin/Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c == null) return NotFound();

        var vm = new CategoryFormVm
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            IsActive = c.IsActive
        };

        await LoadParentOptions(vm, excludeId: id);
        return View(vm);
    }

    // POST: /Admin/Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormVm vm)
    {
        if (id != vm.Id) return BadRequest();

        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();

        vm.Slug = (vm.Slug ?? "").Trim();
        vm.Name = (vm.Name ?? "").Trim();

        if (string.IsNullOrWhiteSpace(vm.Name))
            ModelState.AddModelError(nameof(vm.Name), "Tên không được để trống.");

        vm.Slug = await SlugHelper.GenerateUniqueSlugAsync(
            _db.Categories,
            vm.Name,
            x => x.Slug,
            x => x.Id,
            excludeId: id
        );

        // parent cannot be itself
        if (vm.ParentId.HasValue && vm.ParentId.Value == id)
            ModelState.AddModelError(nameof(vm.ParentId), "Không thể chọn chính nó làm danh mục cha.");

        // prevent cycle: ParentId không được là con/cháu của chính nó
        if (vm.ParentId.HasValue)
        {
            var descendants = await GetDescendantIds(id);
            if (descendants.Contains(vm.ParentId.Value))
                ModelState.AddModelError(nameof(vm.ParentId), "Không thể chọn danh mục con/cháu làm cha (gây vòng lặp).");
        }

        if (!ModelState.IsValid)
        {
            await LoadParentOptions(vm, excludeId: id);
            return View(vm);
        }

        c.Name = vm.Name;
        c.Slug = vm.Slug;
        c.Description = vm.Description;
        c.ParentId = vm.ParentId;
        c.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();

        TempData["ok"] = "Đã cập nhật danh mục.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    // POST: /Admin/Categories/ToggleActive/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();

        c.IsActive = !c.IsActive;
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", new { area = "Admin" });
    }

    // GET: /Admin/Categories/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        return View(c);
    }

    // POST: /Admin/Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var c = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();

        // chặn xoá nếu còn con
        var hasChild = await _db.Categories.AsNoTracking().AnyAsync(x => x.ParentId == id);
        if (hasChild)
        {
            TempData["err"] = "Không thể xoá vì danh mục còn danh mục con.";
            return RedirectToAction("Index", new { area = "Admin" });
        }

        // chặn xoá nếu còn sản phẩm
        var hasProduct = await _db.Products.AsNoTracking().AnyAsync(p => p.CategoryId == id);
        if (hasProduct)
        {
            TempData["err"] = "Không thể xoá vì danh mục còn sản phẩm.";
            return RedirectToAction("Index", new { area = "Admin" });
        }

        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();

        TempData["ok"] = "Đã xoá danh mục.";
        return RedirectToAction("Index", new { area = "Admin" });
    }

    // ===== helpers =====

    private async Task LoadParentOptions(CategoryFormVm vm, int? excludeId)
    {
        // lấy list categories để chọn parent
        var list = await _db.Categories.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        // loại chính nó khỏi dropdown
        if (excludeId.HasValue)
            list = list.Where(x => x.Id != excludeId.Value).ToList();

        vm.ParentOptions = list
            .Select(x => new CategoryFormVm.ParentOption { Id = x.Id, Text = x.Name })
            .ToList();
    }

    private async Task<HashSet<int>> GetDescendantIds(int id)
    {
        // BFS lấy toàn bộ con/cháu
        var result = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(id);

        var all = await _db.Categories.AsNoTracking()
            .Select(x => new { x.Id, x.ParentId })
            .ToListAsync();

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            var children = all.Where(x => x.ParentId == cur).Select(x => x.Id);

            foreach (var childId in children)
            {
                if (result.Add(childId))
                    queue.Enqueue(childId);
            }
        }

        return result;
    }
}
