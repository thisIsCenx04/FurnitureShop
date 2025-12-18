namespace FurnitureShop.ViewModels.Products;

public sealed class ProductListItemVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";

    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }

    public string? MainImageUrl { get; set; }

    public string CategoryName { get; set; } = "";
    public string CategorySlug { get; set; } = "";
}
