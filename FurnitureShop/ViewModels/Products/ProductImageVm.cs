namespace FurnitureShop.ViewModels.Products;

public sealed class ProductImageVm
{
    public string Url { get; set; } = "";
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
