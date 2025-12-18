namespace FurnitureShop.ViewModels.Products;

public sealed class ProductDetailVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";

    public string CategoryName { get; set; } = "";
    public string CategorySlug { get; set; } = "";

    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? Material { get; set; }
    public string? Dimensions { get; set; }
    public string? Style { get; set; }

    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; }

    public string? MainImageUrl { get; set; }
    public List<ProductImageVm> Gallery { get; set; } = [];
}
