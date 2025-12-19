namespace FurnitureShop.ViewModels.Admin.Products;

public sealed class ProductGalleryItemVm
{
    public long Id { get; set; }
    public string Url { get; set; } = "";
    public string? PublicId { get; set; }
    public bool IsPrimary { get; set; }
}
