namespace FurnitureShop.ViewModels.Admin.Products;

public sealed class ProductListItemVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public string? MainImageUrl { get; set; }
}
