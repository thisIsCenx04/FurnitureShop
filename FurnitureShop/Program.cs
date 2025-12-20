using FurnitureShop.Hubs;
using FurnitureShop.Models;
using FurnitureShop.Services.Auth;
using FurnitureShop.Services.Cart;
using FurnitureShop.Services.Notifications;
using FurnitureShop.Services.Seed;
using FurnitureShop.Services.Upload;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<FurnitureShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FurnitureShop"));
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.Cookie.Name = "FurnitureShop.Session";
    opt.IdleTimeout = TimeSpan.FromHours(2);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "FurnitureShop.Auth";
        opt.LoginPath = "/account/login";
        opt.AccessDeniedPath = "/account/access-denied";
        opt.ExpireTimeSpan = TimeSpan.FromDays(7);
        opt.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// SignalR
builder.Services.AddSignalR();

// Seed service
builder.Services.AddScoped<IDbSeeder, DbSeeder>();

// Auth service
builder.Services.AddScoped<IUserService, UserService>();

//Cart service
builder.Services.AddScoped<ICartService, CartService>();

//Cloudinary service
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

//Notification service
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

// Error pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub endpoint
app.MapHub<NotificationHub>("/hubs/notifications");

// Route Area 
// ADMIN route (lock with AdminOnly)
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
).RequireAuthorization("AdminOnly");

// Generic areas route 
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto seed demo data 
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
    await seeder.SeedAsync(app.Environment);
}

app.Run();
