using FurnitureShop.Helpers;
using FurnitureShop.Models;
using FurnitureShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Scripts;

/// <summary>
/// Script seed d? li?u phong phú cho h? th?ng FurnitureShop
/// Ch?y script này trong Program.cs (ch? dùng trong môi tr??ng DEV)
/// </summary>
public static class SeedRichData
{
    public static async Task ExecuteAsync(FurnitureShopContext db)
    {
        Console.WriteLine("?? B?t ??u ki?m tra và seed d? li?u...");

        // Ki?m tra ?ã có d? li?u ch?a
        var hasUsers = await db.Users.AnyAsync();
        var hasCategories = await db.Categories.AnyAsync();
        var hasProducts = await db.Products.AnyAsync();

        if (!hasUsers)
        {
            await SeedUsersAsync(db);
            hasUsers = true;
        }
        else
        {
            Console.WriteLine("?? Users ?ã t?n t?i, b? qua seed users");
        }

        if (!hasCategories)
        {
            await SeedCategoriesAsync(db);
            hasCategories = true;
        }
        else
        {
            Console.WriteLine("?? Categories ?ã t?n t?i, b? qua seed categories");
        }

        if (!hasProducts)
        {
            await SeedProductsAsync(db);
            hasProducts = true;
        }
        else
        {
            Console.WriteLine("?? Products ?ã t?n t?i, b? qua seed products");
        }

        if (hasProducts)
        {
            await SeedProductImagesAsync(db);
        }

        // Ch? seed carts, orders, notifications n?u có ?? d? li?u c? b?n
        if (hasUsers && hasProducts)
        {
            var hasOrders = await db.Orders.AnyAsync();
            if (!hasOrders)
            {
                await SeedCartsAsync(db);
                await SeedOrdersAsync(db);
                await SeedNotificationsAsync(db);
            }
            else
            {
                Console.WriteLine("?? Orders ?ã t?n t?i, b? qua seed carts/orders/notifications");
            }
        }

        Console.WriteLine("? Hoàn thành ki?m tra và seed d? li?u!");
    }

    #region 1. Seed Users
    private static async Task SeedUsersAsync(FurnitureShopContext db)
    {
        Console.WriteLine("Seeding Users...");

        var users = new List<User>
        {
            // Admin accounts
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "admin",
                Email = "admin@furniture.local",
                Phone = "0901234567",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "Qu?n tr? viên",
                Address = "123 Nguy?n Hu?, Q1, TP.HCM",
                Role = 1, // Admin
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "admin2",
                Email = "admin2@furniture.local",
                Phone = "0901234568",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "Nguy?n V?n Admin",
                Address = "456 Lê L?i, Q1, TP.HCM",
                Role = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },

            // Customer accounts
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "customer1",
                Email = "customer1@gmail.com",
                Phone = "0912345678",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Tr?n Th? H??ng",
                Address = "789 Hai Bà Tr?ng, Q3, TP.HCM",
                Role = 0, // Customer
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "customer2",
                Email = "customer2@gmail.com",
                Phone = "0923456789",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Lê V?n Minh",
                Address = "321 Tr?n H?ng ??o, Q5, TP.HCM",
                Role = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "customer3",
                Email = "customer3@gmail.com",
                Phone = "0934567890",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Ph?m Th? Lan",
                Address = "159 Võ V?n T?n, Q3, TP.HCM",
                Role = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "customer4",
                Email = "customer4@gmail.com",
                Phone = "0945678901",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Hoàng V?n Nam",
                Address = "753 Nguy?n Trãi, Q1, TP.HCM",
                Role = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "customer5",
                Email = "customer5@gmail.com",
                Phone = "0961112233",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Pham Thi Ha",
                Address = "12 Nguyen Thi Minh Khai, Q1, TP.HCM",
                Role = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "customer6",
                Email = "customer6@gmail.com",
                Phone = "0972223344",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Tran Gia Huy",
                Address = "88 Le Van Sy, Q3, TP.HCM",
                Role = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = "inactiveuser",
                Email = "inactive@gmail.com",
                Phone = "0956789012",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                FullName = "Ng??i dùng b? khóa",
                Address = "999 Lý Th??ng Ki?t, Q10, TP.HCM",
                Role = 0,
                IsActive = false, // Inactive user
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            }
        };

        db.Users.AddRange(users);
        await db.SaveChangesAsync();
        Console.WriteLine($"? ?ã t?o {users.Count} users");
    }
    #endregion

    #region 2. Seed Categories
    private static async Task SeedCategoriesAsync(FurnitureShopContext db)
    {
        Console.WriteLine("Seeding Categories...");

        // Root categories
        var livingRoom = new Category
        {
            Name = "Phòng khách",
            Slug = SlugHelper.GenerateSlug("Phòng khách"),
            Description = "N?i th?t phòng khách sang tr?ng, hi?n ??i",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var bedroom = new Category
        {
            Name = "Phòng ng?",
            Slug = SlugHelper.GenerateSlug("Phòng ng?"),
            Description = "N?i th?t phòng ng? ?m cúng, ti?n nghi",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var diningRoom = new Category
        {
            Name = "Phòng ?n",
            Slug = SlugHelper.GenerateSlug("Phòng ?n"),
            Description = "B? bàn ?n ??p cho gia ?ình",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var office = new Category
        {
            Name = "V?n phòng",
            Slug = SlugHelper.GenerateSlug("V?n phòng"),
            Description = "N?i th?t v?n phòng chuyên nghi?p",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var outdoor = new Category
        {
            Name = "Ngoài tr?i",
            Slug = SlugHelper.GenerateSlug("Ngoài tr?i"),
            Description = "N?i th?t sân v??n, ban công",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var storage = new Category
        {
            Name = "T? & K?",
            Slug = SlugHelper.GenerateSlug("T? & K?"),
            Description = "Gi?i pháp l?u tr? thông minh",
            ParentId = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Categories.AddRange(livingRoom, bedroom, diningRoom, office, outdoor, storage);
        await db.SaveChangesAsync();

        // Child categories - Phòng khách
        var sofa = new Category
        {
            Name = "Sofa",
            Slug = SlugHelper.GenerateSlug("Sofa"),
            Description = "Gh? sofa cao c?p",
            ParentId = livingRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var coffeeTable = new Category
        {
            Name = "Bàn trà",
            Slug = SlugHelper.GenerateSlug("Bàn trà"),
            Description = "Bàn trà phòng khách",
            ParentId = livingRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tvStand = new Category
        {
            Name = "K? TV",
            Slug = SlugHelper.GenerateSlug("K? TV"),
            Description = "K? tivi hi?n ??i",
            ParentId = livingRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var livingChair = new Category
        {
            Name = "Gh? th? giãn",
            Slug = SlugHelper.GenerateSlug("Gh? th? giãn"),
            Description = "Gh? armchair, gh? ??c sách",
            ParentId = livingRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Child categories - Phòng ng?
        var bed = new Category
        {
            Name = "Gi??ng ng?",
            Slug = SlugHelper.GenerateSlug("Gi??ng ng?"),
            Description = "Gi??ng ng? các lo?i",
            ParentId = bedroom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var wardrobe = new Category
        {
            Name = "T? qu?n áo",
            Slug = SlugHelper.GenerateSlug("T? qu?n áo"),
            Description = "T? áo g? cao c?p",
            ParentId = bedroom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var nightstand = new Category
        {
            Name = "T? ??u gi??ng",
            Slug = SlugHelper.GenerateSlug("T? ??u gi??ng"),
            Description = "Tab ??u gi??ng ti?n d?ng",
            ParentId = bedroom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var dresser = new Category
        {
            Name = "Bàn trang ?i?m",
            Slug = SlugHelper.GenerateSlug("Bàn trang ?i?m"),
            Description = "Bàn trang ?i?m có g??ng",
            ParentId = bedroom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Child categories - Phòng ?n
        var diningTable = new Category
        {
            Name = "Bàn ?n",
            Slug = SlugHelper.GenerateSlug("Bàn ?n"),
            Description = "Bàn ?n gia ?ình",
            ParentId = diningRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var diningChair = new Category
        {
            Name = "Gh? ?n",
            Slug = SlugHelper.GenerateSlug("Gh? ?n"),
            Description = "Gh? ?n ??p, êm ái",
            ParentId = diningRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var buffet = new Category
        {
            Name = "T? b?p",
            Slug = SlugHelper.GenerateSlug("T? b?p"),
            Description = "T? chén bát, buffet",
            ParentId = diningRoom.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Child categories - V?n phòng
        var desk = new Category
        {
            Name = "Bàn làm vi?c",
            Slug = SlugHelper.GenerateSlug("Bàn làm vi?c"),
            Description = "Bàn làm vi?c t?i nhà",
            ParentId = office.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var officeChair = new Category
        {
            Name = "Gh? v?n phòng",
            Slug = SlugHelper.GenerateSlug("Gh? v?n phòng"),
            Description = "Gh? xoay, gh? gaming",
            ParentId = office.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var bookshelf = new Category
        {
            Name = "K? sách",
            Slug = SlugHelper.GenerateSlug("K? sách"),
            Description = "Giá sách v?n phòng",
            ParentId = office.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Child categories - Ngoài tr?i
        var outdoorTable = new Category
        {
            Name = "Bàn sân v??n",
            Slug = SlugHelper.GenerateSlug("Bàn sân v??n"),
            Description = "Bàn cho khu v?c ngoài tr?i",
            ParentId = outdoor.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var outdoorChair = new Category
        {
            Name = "Gh? sân v??n",
            Slug = SlugHelper.GenerateSlug("Gh? sân v??n"),
            Description = "Gh? ban công, sân v??n",
            ParentId = outdoor.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Categories.AddRange(
            sofa, coffeeTable, tvStand, livingChair,
            bed, wardrobe, nightstand, dresser,
            diningTable, diningChair, buffet,
            desk, officeChair, bookshelf,
            outdoorTable, outdoorChair
        );

        await db.SaveChangesAsync();
        Console.WriteLine("? ?ã t?o 6 danh m?c cha và 16 danh m?c con");
    }
    #endregion

    #region 3. Seed Products
    private static async Task SeedProductsAsync(FurnitureShopContext db)
    {
        Console.WriteLine("Seeding Products...");

        var categories = await db.Categories.ToListAsync();
        var sofa = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Sofa"));
        var coffeeTable = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Bàn trà"));
        var bed = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Gi??ng ng?"));
        var wardrobe = categories.First(c => c.Slug == SlugHelper.GenerateSlug("T? qu?n áo"));
        var diningTable = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Bàn ?n"));
        var desk = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Bàn làm vi?c"));
        var officeChair = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Gh? v?n phòng"));
        var tvStand = categories.First(c => c.Slug == SlugHelper.GenerateSlug("K? TV"));
        var nightstand = categories.First(c => c.Slug == SlugHelper.GenerateSlug("T? ??u gi??ng"));
        var bookshelf = categories.First(c => c.Slug == SlugHelper.GenerateSlug("K? sách"));
        var livingChair = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Gh? th? giãn"));
        var dresser = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Bàn trang ?i?m"));
        var diningChair = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Gh? ?n"));
        var buffet = categories.First(c => c.Slug == SlugHelper.GenerateSlug("T? b?p"));
        var outdoorTable = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Bàn sân v??n"));
        var outdoorChair = categories.First(c => c.Slug == SlugHelper.GenerateSlug("Gh? sân v??n"));
        var storage = categories.First(c => c.Slug == SlugHelper.GenerateSlug("T? & K?"));


        var products = new List<Product>
        {
            // Sofa
            new Product
            {
                CategoryId = sofa.Id,
                Name = "Sofa Da Cao C?p Luxury",
                Slug = "sofa-da-cao-cap-luxury",
                ShortDescription = "Sofa da th?t Ý cao c?p, thi?t k? sang tr?ng",
                Description = "Sofa da th?t nh?p kh?u t? Ý, khung g? s?i ch?c ch?n, ??m mút cao c?p êm ái. Thi?t k? hi?n ??i phù h?p m?i không gian phòng khách.",
                Material = "Da th?t Ý, G? s?i",
                Dimensions = "220x95x85 cm",
                Style = "Hi?n ??i",
                Price = 25000000,
                SalePrice = 22000000,
                Stock = 15,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new Product
            {
                CategoryId = sofa.Id,
                Name = "Sofa V?i B? Scandinavian",
                Slug = "sofa-vai-bo-scandinavian",
                ShortDescription = "Sofa v?i b? phong cách B?c Âu t?i gi?n",
                Description = "Thi?t k? t?i gi?n B?c Âu v?i v?i b? cao c?p, màu s?c nh? nhàng. Phù h?p cho không gian hi?n ??i.",
                Material = "V?i b?, G? t?n bì",
                Dimensions = "200x85x80 cm",
                Style = "Scandinavian",
                Price = 12000000,
                SalePrice = 10500000,
                Stock = 25,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1540574163026-643ea20ade25?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            new Product
            {
                CategoryId = sofa.Id,
                Name = "Sofa Góc L Phòng Khách",
                Slug = "sofa-goc-l-phong-khach",
                ShortDescription = "Sofa góc ch? L l?n cho gia ?ình ?ông ng??i",
                Description = "Sofa góc ch? L thi?t k? thông minh ti?t ki?m không gian. Ch?t li?u v?i nhung cao c?p, ??m êm ái.",
                Material = "V?i nhung, G? công nghi?p",
                Dimensions = "280x180x85 cm",
                Style = "Hi?n ??i",
                Price = 18000000,
                SalePrice = null,
                Stock = 12,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1550581190-9c1c48d21d6c?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },

            // Bàn trà
            new Product
            {
                CategoryId = coffeeTable.Id,
                Name = "Bàn Trà G? Óc Chó T? Nhiên",
                Slug = "ban-tra-go-oc-cho-tu-nhien",
                ShortDescription = "Bàn trà g? óc chó nguyên kh?i cao c?p",
                Description = "Bàn trà làm t? g? óc chó t? nhiên, vân g? ??p, b? m?t hoàn thi?n bóng m?n. Thi?t k? chân kim lo?i ch?c ch?n.",
                Material = "G? óc chó, Chân kim lo?i",
                Dimensions = "120x60x45 cm",
                Style = "T?i gi?n",
                Price = 8500000,
                SalePrice = 7500000,
                Stock = 20,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1565191999001-551c187427bb?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
            new Product
            {
                CategoryId = coffeeTable.Id,
                Name = "Bàn Trà M?t Kính C??ng L?c",
                Slug = "ban-tra-mat-kinh-cuong-luc",
                ShortDescription = "Bàn trà m?t kính hi?n ??i",
                Description = "M?t kính c??ng l?c an toàn, chân inox 304 cao c?p không g?. Thi?t k? t?i gi?n phù h?p phòng khách hi?n ??i.",
                Material = "Kính c??ng l?c, Inox 304",
                Dimensions = "100x50x40 cm",
                Style = "Hi?n ??i",
                Price = 3500000,
                SalePrice = null,
                Stock = 30,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },

            // Gi??ng ng?
            new Product
            {
                CategoryId = bed.Id,
                Name = "Gi??ng Ng? G? S?i 1m8",
                Slug = "giuong-ngu-go-soi-1m8",
                ShortDescription = "Gi??ng ng? g? s?i M? cao c?p kích th??c 1m8",
                Description = "Gi??ng g? s?i M? nguyên kh?i, thi?t k? ??u gi??ng b?c n?m sang tr?ng. ?? b?n cao, ch?ng m?i m?t.",
                Material = "G? s?i M?, N?m b?c",
                Dimensions = "200x180x110 cm",
                Style = "Tân c? ?i?n",
                Price = 15000000,
                SalePrice = 13500000,
                Stock = 10,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new Product
            {
                CategoryId = bed.Id,
                Name = "Gi??ng G? Công Nghi?p MDF 1m6",
                Slug = "giuong-go-cong-nghiep-mdf-1m6",
                ShortDescription = "Gi??ng MDF ph? Melamine giá t?t",
                Description = "Gi??ng làm t? g? công nghi?p MDF cao c?p ph? Melamine ch?ng n??c. Thi?t k? ??n gi?n, l?p ráp d? dàng.",
                Material = "MDF ph? Melamine",
                Dimensions = "200x160x100 cm",
                Style = "Hi?n ??i",
                Price = 5500000,
                SalePrice = null,
                Stock = 35,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1578898886969-682513eb4af2?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },

            // T? qu?n áo
            new Product
            {
                CategoryId = wardrobe.Id,
                Name = "T? Áo Cánh Lùa 2m4",
                Slug = "tu-ao-canh-lua-2m4",
                ShortDescription = "T? áo cánh lùa hi?n ??i ti?t ki?m không gian",
                Description = "T? áo 3 cánh lùa v?i g??ng soi toàn thân. Ng?n chia h?p lý, ray tr??t êm ái. Ch?t li?u g? công nghi?p cao c?p.",
                Material = "MDF ph? Melamine, G??ng",
                Dimensions = "240x60x220 cm",
                Style = "Hi?n ??i",
                Price = 12000000,
                SalePrice = 10800000,
                Stock = 8,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-48)
            },
            new Product
            {
                CategoryId = wardrobe.Id,
                Name = "T? Áo G? T?n Bì 4 Cánh",
                Slug = "tu-ao-go-tan-bi-4-canh",
                ShortDescription = "T? áo g? t?n bì t? nhiên 4 cánh m?",
                Description = "T? áo g? t?n bì t? nhiên v?i 4 cánh m?, thi?t k? c? ?i?n sang tr?ng. Không gian l?u tr? l?n v?i nhi?u ng?n chia.",
                Material = "G? t?n bì t? nhiên",
                Dimensions = "200x60x200 cm",
                Style = "Tân c? ?i?n",
                Price = 18000000,
                SalePrice = null,
                Stock = 5,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1595428774223-ef52624120d2?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },

            // Bàn ?n
            new Product
            {
                CategoryId = diningTable.Id,
                Name = "B? Bàn ?n 6 Gh? G? S?i",
                Slug = "bo-ban-an-6-ghe-go-soi",
                ShortDescription = "B? bàn ?n 6 gh? g? s?i sang tr?ng",
                Description = "B? bàn ?n g? s?i t? nhiên g?m 1 bàn và 6 gh?. Thi?t k? ch?c ch?n, phù h?p cho gia ?ình 6-8 ng??i.",
                Material = "G? s?i t? nhiên",
                Dimensions = "160x90x75 cm",
                Style = "Hi?n ??i",
                Price = 14000000,
                SalePrice = 12500000,
                Stock = 12,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1617806118233-18e1de247200?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
            new Product
            {
                CategoryId = diningTable.Id,
                Name = "Bàn ?n M?t ?á Marble",
                Slug = "ban-an-mat-da-marble",
                ShortDescription = "Bàn ?n m?t ?á marble cao c?p",
                Description = "Bàn ?n v?i m?t ?á marble t? nhiên sang tr?ng, chân kim lo?i s?n t?nh ?i?n. D? v? sinh, b?n ??p theo th?i gian.",
                Material = "?á marble, Chân kim lo?i",
                Dimensions = "140x80x75 cm",
                Style = "Sang tr?ng",
                Price = 22000000,
                SalePrice = null,
                Stock = 6,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1595428774223-ef52624120d2?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-42)
            },

            // Bàn làm vi?c
            new Product
            {
                CategoryId = desk.Id,
                Name = "Bàn Làm Vi?c G? Cao Su",
                Slug = "ban-lam-viec-go-cao-su",
                ShortDescription = "Bàn làm vi?c g? cao su t? nhiên",
                Description = "Bàn làm vi?c g? cao su v?i 2 ng?n kéo ti?n d?ng. B? m?t r?ng rãi phù h?p làm vi?c và h?c t?p.",
                Material = "G? cao su t? nhiên",
                Dimensions = "120x60x75 cm",
                Style = "T?i gi?n",
                Price = 3500000,
                SalePrice = 3200000,
                Stock = 40,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new Product
            {
                CategoryId = desk.Id,
                Name = "Bàn Gaming RGB Cao C?p",
                Slug = "ban-gaming-rgb-cao-cap",
                ShortDescription = "Bàn gaming v?i ?èn LED RGB",
                Description = "Bàn gaming chuyên d?ng v?i ?èn LED RGB 16 tri?u màu, móc treo tai nghe, giá ?? c?c. M?t bàn ch?ng n??c, ch?ng tr?y.",
                Material = "MDF ch?ng n??c, Kim lo?i",
                Dimensions = "140x70x75 cm",
                Style = "Gaming",
                Price = 6500000,
                SalePrice = null,
                Stock = 18,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1595515106969-1ce29566ff1c?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },

            // Gh? v?n phòng
            new Product
            {
                CategoryId = officeChair.Id,
                Name = "Gh? Xoay L?ng L??i Ergonomic",
                Slug = "ghe-xoay-lung-luoi-ergonomic",
                ShortDescription = "Gh? v?n phòng ergonomic ch?ng ?au l?ng",
                Description = "Gh? xoay l?ng l??i v?i thi?t k? ergonomic h? tr? c?t s?ng. Tay v?n ?i?u ch?nh 3D, nâng h? b?ng khí nén.",
                Material = "L??i cao c?p, Chân nh?a",
                Dimensions = "65x65x115 cm",
                Style = "Hi?n ??i",
                Price = 2500000,
                SalePrice = 2200000,
                Stock = 50,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1580480055273-228ff5388ef8?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-22)
            },
            new Product
            {
                CategoryId = officeChair.Id,
                Name = "Gh? Gaming Pro E-Sport",
                Slug = "ghe-gaming-pro-e-sport",
                ShortDescription = "Gh? gaming chuyên nghi?p cho game th?",
                Description = "Gh? gaming cao c?p b?c da PU, ??m foam density cao. T?a l?ng ng? 180 ??, g?i t?a ??u và l?ng ?i kèm.",
                Material = "Da PU, Khung thép",
                Dimensions = "70x70x130 cm",
                Style = "Gaming",
                Price = 4500000,
                SalePrice = 3900000,
                Stock = 25,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1598550476439-6847785fcea6?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },

            // K? TV
            new Product
            {
                CategoryId = tvStand.Id,
                Name = "K? Tivi G? Óc Chó 1m8",
                Slug = "ke-tivi-go-oc-cho-1m8",
                ShortDescription = "K? tivi g? óc chó hi?n ??i",
                Description = "K? tivi v?i 2 ng?n kéo và 2 khoang m?. Ch?t li?u g? óc chó t? nhiên vân ??p. Phù h?p TV t? 55-65 inch.",
                Material = "G? óc chó t? nhiên",
                Dimensions = "180x45x50 cm",
                Style = "Hi?n ??i",
                Price = 9500000,
                SalePrice = null,
                Stock = 15,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-38)
            },

            // T? ??u gi??ng
            new Product
            {
                CategoryId = nightstand.Id,
                Name = "Tab ??u Gi??ng 2 Ng?n Kéo",
                Slug = "tab-dau-giuong-2-ngan-keo",
                ShortDescription = "Tab ??u gi??ng nh? g?n ti?n d?ng",
                Description = "T? ??u gi??ng v?i 2 ng?n kéo ray tr??t êm. Thi?t k? nh? g?n phù h?p nhi?u không gian.",
                Material = "MDF ph? Melamine",
                Dimensions = "50x40x50 cm",
                Style = "T?i gi?n",
                Price = 1200000,
                SalePrice = 1000000,
                Stock = 60,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1556228578-8c89e6adf883?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },

            // K? sách
            new Product
            {
                CategoryId = bookshelf.Id,
                Name = "Giá Sách 5 T?ng G? Cao Su",
                Slug = "gia-sach-5-tang-go-cao-su",
                ShortDescription = "K? sách 5 t?ng ch?c ch?n",
                Description = "Giá sách g? cao su v?i 5 t?ng, m?i t?ng ch?u t?i 15kg. Thi?t k? ??n gi?n d? l?p ráp.",
                Material = "G? cao su t? nhiên",
                Dimensions = "80x30x180 cm",
                Style = "Hi?n ??i",
                Price = 2800000,
                SalePrice = null,
                Stock = 22,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1594026112284-02bb6f3352fe?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-32)
            },

            // Ghe thu gian
            new Product
            {
                CategoryId = livingChair.Id,
                Name = "Gh? th? gi?n b?c n?",
                Slug = "ghe-thu-gian-boc-ni",
                ShortDescription = "Gh? armchair nh? g?n, ??m m?m",
                Description = "Gh? th? gi?n b?c n?, khung go chac chan, ngoi em. Phu hop phong khach va phong doc.",
                Material = "V?i n?, khung g?",
                Dimensions = "75x80x95 cm",
                Style = "Modern",
                Price = 3200000,
                SalePrice = 2900000,
                Stock = 28,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1501045661006-fcebe0257c3f?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-27)
            },

            // Ban trang diem
            new Product
            {
                CategoryId = dresser.Id,
                Name = "B?n trang ?i?m g? th?ng",
                Slug = "ban-trang-diem-go-thong",
                ShortDescription = "B?n trang ?i?m c? g??ng, 2 ng?n k?o",
                Description = "B?n trang ?i?m g? th?ng chong am, guong gap gon, 2 ngan keo tien dung.",
                Material = "G? th?ng, g??ng",
                Dimensions = "100x40x140 cm",
                Style = "Minimal",
                Price = 4800000,
                SalePrice = 4500000,
                Stock = 14,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-35)
            },

            // Ghe an
            new Product
            {
                CategoryId = diningChair.Id,
                Name = "Gh? ?n b?c n? ch?n g?",
                Slug = "ghe-an-boc-ni-chan-go",
                ShortDescription = "Gh? ?n b?c n?, ch?n g? s?i",
                Description = "Gh? ?n b?c n? cao c?p, ch?n g? s?i ch?c ch?n, ng?i ?m.",
                Material = "V?i n?, g? s?i",
                Dimensions = "45x50x90 cm",
                Style = "Scandinavian",
                Price = 1200000,
                SalePrice = 990000,
                Stock = 80,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-33)
            },

            // Tu buffet
            new Product
            {
                CategoryId = buffet.Id,
                Name = "T? buffet g? s?i 1m6",
                Slug = "tu-buffet-go-soi-1m6",
                ShortDescription = "T? buffet g? s?i, 3 ng?n k?o",
                Description = "T? buffet g? s?i 1m6, mat go day, nhieu ngan keo luu tru.",
                Material = "G? s?i t? nhi?n",
                Dimensions = "160x45x80 cm",
                Style = "Modern",
                Price = 7800000,
                SalePrice = 7200000,
                Stock = 12,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-29)
            },

            // Ban san vuon
            new Product
            {
                CategoryId = outdoorTable.Id,
                Name = "B?n s?n v??n nh?m ??c",
                Slug = "ban-san-vuon-nhom-duc",
                ShortDescription = "B?n nh?m ??c ngo?i tr?i, ch?ng r?",
                Description = "B?n s?n v??n nh?m ??c, son tinh dien, chong ri set, de ve sinh.",
                Material = "Nh?m ??c, s?n t?nh ?i?n",
                Dimensions = "120x70x75 cm",
                Style = "Outdoor",
                Price = 5600000,
                SalePrice = null,
                Stock = 10,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-21)
            },

            // Ghe san vuon
            new Product
            {
                CategoryId = outdoorChair.Id,
                Name = "Gh? s?n v??n g?p g?n",
                Slug = "ghe-san-vuon-gap-gon",
                ShortDescription = "Gh? g?p g?n cho ban c?ng, s?n v??n",
                Description = "Gh? s?n v??n g?p g?n, khung thep son tinh dien, de xep gon.",
                Material = "Th?p s?n t?nh ?i?n, v?i Textilene",
                Dimensions = "55x60x85 cm",
                Style = "Outdoor",
                Price = 850000,
                SalePrice = null,
                Stock = 40,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-19)
            },

            // Tu & Ke
            new Product
            {
                CategoryId = storage.Id,
                Name = "K? trang tr? 4 t?ng ?a n?ng",
                Slug = "ke-trang-tri-4-tang-da-nang",
                ShortDescription = "K? 4 t?ng ?? trang tr? v? l?u tr?",
                Description = "K? trang tr? 4 t?ng, khung s?t s?n t?nh ?i?n, g? MDF ch?ng ?m.",
                Material = "MDF ch?ng ?m, khung s?t",
                Dimensions = "80x30x160 cm",
                Style = "Industrial",
                Price = 1900000,
                SalePrice = 1700000,
                Stock = 25,
                IsActive = true,
                MainImageUrl = "https://images.unsplash.com/photo-1493663284031-b7e3aefcae8e?w=800",
                CreatedAt = DateTime.UtcNow.AddDays(-24)
            },

            // S?n ph?m inactive (out of stock)
            new Product
            {
                CategoryId = sofa.Id,
                Name = "Sofa C? ?i?n Châu Âu",
                Slug = "sofa-co-dien-chau-au",
                ShortDescription = "Sofa c? ?i?n phong cách châu Âu",
                Description = "?ã ng?ng kinh doanh",
                Material = "G? g?, Da bò",
                Dimensions = "230x100x90 cm",
                Style = "C? ?i?n",
                Price = 35000000,
                SalePrice = null,
                Stock = 0,
                IsActive = false,
                MainImageUrl = null,
                CreatedAt = DateTime.UtcNow.AddDays(-100)
            }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
        Console.WriteLine($"? ?ã t?o {products.Count} s?n ph?m");
    }
    #endregion

    #region 3.5. Seed Product Images
    private static async Task SeedProductImagesAsync(FurnitureShopContext db)
    {
        Console.WriteLine("?? Seeding ProductImages...");

        var products = await db.Products.AsNoTracking().ToListAsync();
        if (!products.Any())
        {
            Console.WriteLine("?? No products to create product images");
            return;
        }

        var existing = await db.ProductImages.AsNoTracking().Select(p => p.ProductId).ToListAsync();
        var existingSet = new HashSet<int>(existing);

        const string fallbackUrl = "https://images.unsplash.com/photo-1501045661006-fcebe0257c3f?w=800";

        var images = new List<ProductImage>();
        foreach (var product in products)
        {
            if (existingSet.Contains(product.Id))
                continue;

            var imageUrl = string.IsNullOrWhiteSpace(product.MainImageUrl) ? fallbackUrl : product.MainImageUrl!;
            var publicId = $"seed-{product.Slug}";
            if (publicId.Length > 200)
            {
                publicId = publicId[..200];
            }

            images.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = imageUrl,
                PublicId = publicId,
                IsPrimary = true,
                SortOrder = 0,
                CreatedAt = product.CreatedAt
            });
        }

        if (!images.Any())
        {
            Console.WriteLine("?? ProductImages already exist for all products");
            return;
        }

        db.ProductImages.AddRange(images);
        await db.SaveChangesAsync();
        Console.WriteLine($"? Created {images.Count} product images");
    }
    #endregion

    #region 4. Seed Carts & Cart Items
    private static async Task SeedCartsAsync(FurnitureShopContext db)
    {
        Console.WriteLine("Seeding Carts...");

        var users = await db.Users.Where(u => u.Role == 0 && u.IsActive).ToListAsync();
        var products = await db.Products.Where(p => p.IsActive).Take(10).ToListAsync();

        if (!users.Any() || !products.Any())
        {
            Console.WriteLine("?? Không có users ho?c products ?? t?o carts");
            return;
        }

        var carts = new List<Cart>();
        var cartItems = new List<CartItem>();

        // Cart cho user ?ã ??ng nh?p
        foreach (var user in users.Take(3))
        {
            var cart = new Cart
            {
                UserId = user.Id,
                CartKey = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            };
            carts.Add(cart);
        }

        // Cart cho guest (không có UserId)
        for (int i = 0; i < 2; i++)
        {
            var guestCart = new Cart
            {
                UserId = null,
                CartKey = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow.AddHours(-i * 6),
                UpdatedAt = DateTime.UtcNow.AddHours(-i * 6)
            };
            carts.Add(guestCart);
        }

        db.Carts.AddRange(carts);
        await db.SaveChangesAsync();

        // Thêm items vào cart
        var random = new Random();
        foreach (var cart in carts)
        {
            int itemCount = random.Next(1, 4); // 1-3 items per cart
            var selectedProducts = products.OrderBy(x => random.Next()).Take(itemCount).ToList();

            foreach (var product in selectedProducts)
            {
                cartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = random.Next(1, 4),
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 5))
                });
            }
        }

        db.CartItems.AddRange(cartItems);
        await db.SaveChangesAsync();
        Console.WriteLine($"? ?ã t?o {carts.Count} gi? hàng v?i {cartItems.Count} items");
    }
    #endregion

    #region 5. Seed Orders
    private static async Task SeedOrdersAsync(FurnitureShopContext db)
    {
        Console.WriteLine("Seeding Orders...");

        var users = await db.Users.Where(u => u.Role == 0 && u.IsActive).ToListAsync();
        var products = await db.Products.Where(p => p.IsActive).Take(15).ToListAsync();
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Role == 1);

        if (!users.Any() || !products.Any() || admin == null)
        {
            Console.WriteLine("?? Không ?? d? li?u ?? t?o orders");
            return;
        }

        var orders = new List<Order>();
        var orderItems = new List<OrderItem>();
        var statusHistories = new List<OrderStatusHistory>();

        var random = new Random();
        int orderCount = 20;

        for (int i = 0; i < orderCount; i++)
        {
            var user = users[random.Next(users.Count)];
            var createdDate = DateTime.UtcNow.AddDays(-random.Next(1, 90));
            
            // Random status: 0=Pending, 1=Confirmed, 2=Processing, 3=Shipping, 4=Completed, 5=Cancelled
            byte status = (byte)random.Next(0, 6);
            
            // ??n hàng c? h?n th??ng ?ã hoàn thành
            if (createdDate < DateTime.UtcNow.AddDays(-30))
                status = (byte)random.Next(4, 6); // Completed or Cancelled

            var order = new Order
            {
                OrderCode = $"ORD{DateTime.UtcNow.Ticks.ToString().Substring(8)}{i:D3}",
                UserId = user.Id,
                CustomerName = user.FullName ?? "Khách hàng",
                Phone = user.Phone ?? "0900000000",
                Email = user.Email,
                Address = user.Address ?? "??a ch? giao hàng",
                Note = random.Next(0, 3) == 0 ? "Giao hàng gi? hành chính" : null,
                PaymentMethod = (byte)random.Next(0, 2), // 0=COD, 1=Bank Transfer
                Status = status,
                Subtotal = 0, // Will calculate
                ShippingFee = 50000,
                DiscountAmount = 0,
                Total = 0, // Will calculate
                CreatedAt = createdDate,
                UpdatedAt = status > 0 ? createdDate.AddHours(random.Next(1, 48)) : null
            };

            // Thêm 1-4 s?n ph?m vào ??n hàng
            int itemCount = random.Next(1, 5);
            var selectedProducts = products.OrderBy(x => random.Next()).Take(itemCount).ToList();
            decimal subtotal = 0;

            foreach (var product in selectedProducts)
            {
                int quantity = random.Next(1, 3);
                decimal unitPrice = product.SalePrice ?? product.Price;
                decimal lineTotal = unitPrice * quantity;
                subtotal += lineTotal;

                orderItems.Add(new OrderItem
                {
                    Order = order,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = unitPrice,      // ? S?a: Price -> UnitPrice
                    Quantity = quantity,
                    LineTotal = lineTotal,      // ? S?a: Subtotal -> LineTotal
                    CreatedAt = createdDate
                });
            }

            order.Subtotal = subtotal;
            order.Total = subtotal + order.ShippingFee - order.DiscountAmount;

            orders.Add(order);

            // T?o l?ch s? tr?ng thái
            statusHistories.Add(new OrderStatusHistory
            {
                Order = order,
                FromStatus = 0,
                ToStatus = 0, // Pending
                ChangedByUserId = user.Id,
                Note = "??n hàng ???c t?o",
                ChangedAt = createdDate
            });

            if (status >= 1)
            {
                statusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    FromStatus = 0,
                    ToStatus = 1, // Confirmed
                    ChangedByUserId = admin.Id,
                    Note = "??n hàng ?ã ???c xác nh?n",
                    ChangedAt = createdDate.AddHours(2)
                });
            }

            if (status >= 2)
            {
                statusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    FromStatus = 1,
                    ToStatus = 2, // Processing
                    ChangedByUserId = admin.Id,
                    Note = "?ang chu?n b? hàng",
                    ChangedAt = createdDate.AddHours(12)
                });
            }

            if (status >= 3)
            {
                statusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    FromStatus = 2,
                    ToStatus = 3, // Shipping
                    ChangedByUserId = admin.Id,
                    Note = "??n hàng ?ang v?n chuy?n",
                    ChangedAt = createdDate.AddHours(24)
                });
            }

            if (status == 4)
            {
                statusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    FromStatus = 3,
                    ToStatus = 4, // Completed
                    ChangedByUserId = user.Id,
                    Note = "?ã nh?n hàng",
                    ChangedAt = createdDate.AddHours(72)
                });
            }

            if (status == 5)
            {
                statusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    FromStatus = (byte)random.Next(0, 3),
                    ToStatus = 5, // Cancelled
                    ChangedByUserId = random.Next(0, 2) == 0 ? user.Id : admin.Id,
                    Note = "??n hàng b? h?y",
                    ChangedAt = createdDate.AddHours(random.Next(1, 24))
                });
            }
        }

        db.Orders.AddRange(orders);
        db.OrderItems.AddRange(orderItems);
        db.OrderStatusHistories.AddRange(statusHistories);
        await db.SaveChangesAsync();

        Console.WriteLine($"? ?ã t?o {orders.Count} ??n hàng v?i {orderItems.Count} items và {statusHistories.Count} l?ch s?");
    }
    #endregion

    #region 6. Seed Notifications
    private static async Task SeedNotificationsAsync(FurnitureShopContext db)
    {
        Console.WriteLine("Seeding Notifications...");

        var users = await db.Users.Where(u => u.Role == 0 && u.IsActive).ToListAsync();
        var orders = await db.Orders.OrderByDescending(o => o.CreatedAt).Take(10).ToListAsync();

        if (!users.Any())
        {
            Console.WriteLine("?? Không có users ?? t?o notifications");
            return;
        }

        var notifications = new List<Notification>();
        var random = new Random();

        // Notifications cho t?ng user
        foreach (var user in users)
        {
            var userOrders = orders.Where(o => o.UserId == user.Id).ToList();

            foreach (var order in userOrders)
            {
                // Notification khi ??n hàng ???c t?o
                notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Type = "order",
                    Title = "??n hàng m?i",
                    Message = $"??n hàng {order.OrderCode} ?ã ???c t?o thành công",
                    Url = $"/orders/{order.Id}",
                    IsRead = random.Next(0, 3) == 0,
                    CreatedAt = order.CreatedAt
                });

                // Notification khi ??n hàng thay ??i tr?ng thái
                if (order.Status >= 1)
                {
                    notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Type = "order",
                        Title = "??n hàng ?ã xác nh?n",
                        Message = $"??n hàng {order.OrderCode} ?ã ???c xác nh?n",
                        Url = $"/orders/{order.Id}",
                        IsRead = random.Next(0, 2) == 0,
                        CreatedAt = order.CreatedAt.AddHours(2)
                    });
                }

                if (order.Status == 4)
                {
                    notifications.Add(new Notification
                    {
                        UserId = user.Id,
                        Type = "order",
                        Title = "Giao hàng thành công",
                        Message = $"??n hàng {order.OrderCode} ?ã ???c giao thành công",
                        Url = $"/orders/{order.Id}",
                        IsRead = true,
                        CreatedAt = order.CreatedAt.AddDays(3)
                    });
                }
            }

            // Thông báo khuy?n mãi
            if (random.Next(0, 2) == 0)
            {
                notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Type = "promotion",
                    Title = "Khuy?n mãi ??c bi?t",
                    Message = "Gi?m giá 20% cho t?t c? s?n ph?m sofa trong tu?n này!",
                    Url = "/products?categories=sofa",
                    IsRead = random.Next(0, 3) == 0,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 7))
                });
            }
        }

        // Notifications chung (system)
        notifications.Add(new Notification
        {
            UserId = null,
            Type = "system",
            Title = "B?o trì h? th?ng",
            Message = "H? th?ng s? b?o trì vào 2h sáng ngày mai",
            Url = null,
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(-12)
        });

        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();

        Console.WriteLine($"? ?ã t?o {notifications.Count} thông báo");
    }
    #endregion
}