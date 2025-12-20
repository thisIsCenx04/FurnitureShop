using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FurnitureShop.ViewModels.Admin.Products;

public sealed class ProductEditVm
{
    public long Id { get; set; }

    [Required] public string Name { get; set; } = "";
    public string Slug { get; set; } = "";

    [Required] public int CategoryId { get; set; }

    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    public string? Material { get; set; }
    public string? Dimensions { get; set; }
    public string? Style { get; set; }

    [Range(0, 999999999)] public decimal Price { get; set; }
    [Range(0, 999999999)] public decimal? SalePrice { get; set; }

    [Range(0, 999999)] public int Stock { get; set; }
    public bool IsActive { get; set; } = true;

    // main image upload
    public IFormFile? MainImageFile { get; set; }

    // gallery uploads
    public List<IFormFile> GalleryFiles { get; set; } = new();

    // show current main image
    public string? CurrentMainImageUrl { get; set; }
    public string? CurrentMainImagePublicId { get; set; }
}
