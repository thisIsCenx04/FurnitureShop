using FurnitureShop.Models.Entities;

namespace FurnitureShop.ViewModels;

public sealed class HomeIndexVm
{
    public List<Category> Categories { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}
