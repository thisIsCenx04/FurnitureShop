using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using FurnitureShop.Services.Upload;
using FurnitureShop.ViewModels.Admin.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class ProductsController : Controller
{
    private readonly FurnitureShopContext _db;
    private readonly ICloudinaryService _cloud;

    public ProductsController(FurnitureShopContext db, ICloudinaryService cloud)
    {
        _db = db;
        _cloud = cloud;
    }

    // GET: /Admin/Products
    public async Task<IActionResult> Index()
    {
        var items = await _db.Products.AsNoTracking()
            .OrderByDescending(p => p.Id)
            .Select(p => new ProductListItemVm
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                MainImageUrl = p.MainImageUrl
            })
            .ToListAsync();

        return View(items);
    }

    // GET: /Admin/Products/Create
    public async Task<IActionResult> Create()
    {
        var vm = new ProductEditPageVm
        {
            Categories = await GetCategorySelectAsync()
        };
        return View(vm);
    }

    // POST: /Admin/Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductEditVm form)
    {
        form.Name = (form.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(form.Name))
            ModelState.AddModelError("Name", "Tên không được để trống.");

        // luôn tự sinh slug từ Name, đảm bảo unique
        form.Slug = await SlugHelper.GenerateUniqueSlugAsync(
            _db.Products,
            form.Name,
            x => x.Slug,
            x => x.Id
        );

        if (!ModelState.IsValid)
            return View(new ProductEditPageVm { Form = form, Categories = await GetCategorySelectAsync() });

        var p = new Product
        {
            Name = form.Name,
            Slug = form.Slug,
            CategoryId = form.CategoryId,
            ShortDescription = form.ShortDescription,
            Description = form.Description,
            Material = form.Material,
            Dimensions = form.Dimensions,
            Style = form.Style,
            Price = form.Price,
            SalePrice = form.SalePrice,
            Stock = form.Stock,
            IsActive = form.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        // Upload main image
        if (form.MainImageFile != null)
        {
            var up = await _cloud.UploadImageAsync(form.MainImageFile, "FurnitureShop/products/main");
            p.MainImageUrl = up.Url;
            p.MainImagePublicId = up.PublicId;
        }

        _db.Products.Add(p);
        await _db.SaveChangesAsync();

        // Upload gallery
        if (form.GalleryFiles.Count > 0)
        {
            await UploadGalleryAsync(p.Id, form.GalleryFiles, setFirstAsPrimaryIfNone: true);
        }

        return RedirectToAction("Index", new { area = "Admin" });
    }

    // GET: /Admin/Products/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        var gallery = await _db.ProductImages.AsNoTracking()
            .Where(x => x.ProductId == id)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .ThenByDescending(x => x.Id)
            .Select(x => new ProductGalleryItemVm
            {
                Id = x.Id,
                Url = x.ImageUrl,
                PublicId = x.PublicId,
                IsPrimary = x.IsPrimary
            })
            .ToListAsync();

        var vm = new ProductEditPageVm
        {
            Form = new ProductEditVm
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                CategoryId = p.CategoryId,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                Material = p.Material,
                Dimensions = p.Dimensions,
                Style = p.Style,
                Price = p.Price,
                SalePrice = p.SalePrice,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CurrentMainImageUrl = p.MainImageUrl,
                CurrentMainImagePublicId = p.MainImagePublicId
            },
            Categories = await GetCategorySelectAsync(),
            Gallery = gallery
        };

        return View(vm);
    }

    // POST: /Admin/Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductEditPageVm vm)
    {
        var form = vm.Form;

        if (id != form.Id) return BadRequest();

        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        form.Name = (form.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(form.Name))
            ModelState.AddModelError("Form.Name", "Tên không được để trống.");

        form.Slug = await SlugHelper.GenerateUniqueSlugAsync(
            _db.Products,
            form.Name,
            x => x.Slug,
            x => x.Id,
            excludeId: id
        );

        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectAsync();
            vm.Gallery = await _db.ProductImages.AsNoTracking()
                .Where(x => x.ProductId == id)
                .Select(x => new ProductGalleryItemVm
                {
                    Id = x.Id,
                    Url = x.ImageUrl,
                    PublicId = x.PublicId,
                    IsPrimary = x.IsPrimary
                }).ToListAsync();

            return View(vm);
        }

        // update entity
        p.Name = form.Name;
        p.Slug = form.Slug;
        p.CategoryId = form.CategoryId;
        p.ShortDescription = form.ShortDescription;
        p.Description = form.Description;
        p.Material = form.Material;
        p.Dimensions = form.Dimensions;
        p.Style = form.Style;
        p.Price = form.Price;
        p.SalePrice = form.SalePrice;
        p.Stock = form.Stock;
        p.IsActive = form.IsActive;

        // replace main image
        if (form.MainImageFile != null)
        {
            if (!string.IsNullOrWhiteSpace(p.MainImagePublicId))
                await _cloud.DeleteAsync(p.MainImagePublicId);

            var up = await _cloud.UploadImageAsync(form.MainImageFile, "FurnitureShop/products/main");
            p.MainImageUrl = up.Url;
            p.MainImagePublicId = up.PublicId;
        }

        await _db.SaveChangesAsync();

        if (form.GalleryFiles.Count > 0)
            await UploadGalleryAsync(p.Id, form.GalleryFiles, true);

        return RedirectToAction("Index", new { area = "Admin"});
    }


    // POST: /Admin/Products/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        // delete main
        if (!string.IsNullOrWhiteSpace(p.MainImagePublicId))
            await _cloud.DeleteAsync(p.MainImagePublicId);

        // delete gallery cloud + db
        var imgs = await _db.ProductImages.Where(x => x.ProductId == id).ToListAsync();
        foreach (var i in imgs)
        {
            if (!string.IsNullOrWhiteSpace(i.PublicId))
                await _cloud.DeleteAsync(i.PublicId);
        }
        _db.ProductImages.RemoveRange(imgs);

        _db.Products.Remove(p);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", new { area = "Admin" });
    }

    // POST: /Admin/Products/SetPrimaryImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)
    {
        // enforce 1 primary
        var imgs = await _db.ProductImages.Where(x => x.ProductId == productId).ToListAsync();
        foreach (var img in imgs)
            img.IsPrimary = (img.Id == imageId);

        await _db.SaveChangesAsync();
        return RedirectToAction("Edit", new { area = "Admin", id = productId });
    }

    // POST: /Admin/Products/DeleteGalleryImage
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGalleryImage(int productId, int imageId)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);
        if (img == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(img.PublicId))
            await _cloud.DeleteAsync(img.PublicId);

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();

        // nếu xoá mất primary thì set lại 1 cái
        await EnsurePrimaryExistsAsync(productId);

        return RedirectToAction("Edit", new { area = "Admin", id = productId });
    }

    // ===== Helpers =====

    private async Task<List<SelectListItem>> GetCategorySelectAsync()
    {
        return await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();
    }

    private async Task UploadGalleryAsync(int productId, List<IFormFile> files, bool setFirstAsPrimaryIfNone)
    {
        var hasPrimary = await _db.ProductImages.AnyAsync(x => x.ProductId == productId && x.IsPrimary);

        foreach (var f in files)
        {
            var up = await _cloud.UploadImageAsync(f, "FurnitureShop/products/gallery");

            var entity = new ProductImage
            {
                ProductId = productId,
                ImageUrl = up.Url,
                PublicId = up.PublicId,
                IsPrimary = false,
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

            // set primary cho ảnh đầu tiên nếu chưa có primary
            if (setFirstAsPrimaryIfNone && !hasPrimary)
            {
                entity.IsPrimary = true;
                hasPrimary = true;
            }

            _db.ProductImages.Add(entity);
        }

        await _db.SaveChangesAsync();
    }

    private async Task EnsurePrimaryExistsAsync(int productId)
    {
        var hasPrimary = await _db.ProductImages.AnyAsync(x => x.ProductId == productId && x.IsPrimary);
        if (hasPrimary) return;

        var first = await _db.ProductImages
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (first == null) return;
        first.IsPrimary = true;
        await _db.SaveChangesAsync();
    }
}
