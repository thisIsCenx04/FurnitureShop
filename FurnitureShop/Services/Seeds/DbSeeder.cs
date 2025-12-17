using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Services.Seed;

public sealed class DbSeeder : IDbSeeder
{
    private readonly FurnitureShopContext _db;

    public DbSeeder(FurnitureShopContext db) => _db = db;

    public async Task SeedAsync(IWebHostEnvironment env)
    {
        // Chỉ seed khi DEV (tránh “đụng dữ liệu thật”)
        if (!env.IsDevelopment()) return;

        // Seed admin user (DEV only)
        if (!await _db.Users.AsNoTracking().AnyAsync())
        {
            _db.Users.Add(new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "admin",
                Email = "admin@furniture.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            _db.Users.Add(new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "user",
                Email = "user@furniture.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }


        // Nếu đã có dữ liệu thì bỏ qua
        var hasAnyCategory = await _db.Categories.AsNoTracking().AnyAsync();
        var hasAnyProduct = await _db.Products.AsNoTracking().AnyAsync();
        if (hasAnyCategory || hasAnyProduct) return;

        // 1) Root categories
        var living = new Category
        {
            Name = "Phòng khách",
            Slug = SlugHelper.GenerateSlug("Phòng khách"),
            Description = "Danh mục cho phòng khách",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var bedroom = new Category
        {
            Name = "Phòng ngủ",
            Slug = SlugHelper.GenerateSlug("Phòng ngủ"),
            Description = "Danh mục cho phòng ngủ",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.AddRange(living, bedroom);
        await _db.SaveChangesAsync();

        // 2) Child categories
        var sofa = new Category
        {
            Name = "Sofa",
            Slug = SlugHelper.GenerateSlug("Sofa"),
            ParentId = living.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var coffeeTable = new Category
        {
            Name = "Bàn trà",
            Slug = SlugHelper.GenerateSlug("Bàn trà"),
            ParentId = living.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var bed = new Category
        {
            Name = "Giường",
            Slug = SlugHelper.GenerateSlug("Giường"),
            ParentId = bedroom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Categories.AddRange(sofa, coffeeTable, bed);
        await _db.SaveChangesAsync();

        // 3) Products demo (MainImageUrl có thể null; view sẽ tự placeholder)
        var products = new List<Product>
        {
            new()
            {
                CategoryId = sofa.Id,
                Name = "Sofa vải hiện đại 3 chỗ",
                Slug = SlugHelper.GenerateSlug("Sofa vải hiện đại 3 chỗ"),
                ShortDescription = "Thiết kế hiện đại, dễ phối nội thất.",
                Material = "Vải bố",
                Dimensions = "220x90x85cm",
                Style = "Modern",
                Price = 12000000m,
                SalePrice = 9900000m,
                Stock = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                CategoryId = coffeeTable.Id,
                Name = "Bàn trà gỗ sồi",
                Slug = SlugHelper.GenerateSlug("Bàn trà gỗ sồi"),
                ShortDescription = "Gọn gàng, chắc chắn.",
                Material = "Gỗ sồi",
                Dimensions = "100x60x45cm",
                Style = "Scandinavian",
                Price = 3500000m,
                SalePrice = null,
                Stock = 20,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                CategoryId = bed.Id,
                Name = "Giường ngủ gỗ thông 1m6",
                Slug = SlugHelper.GenerateSlug("Giường ngủ gỗ thông 1m6"),
                ShortDescription = "Khung chắc, màu gỗ tự nhiên.",
                Material = "Gỗ thông",
                Dimensions = "160x200cm",
                Style = "Minimal",
                Price = 6500000m,
                SalePrice = 5990000m,
                Stock = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _db.Products.AddRange(products);
        await _db.SaveChangesAsync();
    }
}
