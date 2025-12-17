using Microsoft.AspNetCore.Hosting;

namespace FurnitureShop.Services.Seed;

public interface IDbSeeder
{
    Task SeedAsync(IWebHostEnvironment env);
}
