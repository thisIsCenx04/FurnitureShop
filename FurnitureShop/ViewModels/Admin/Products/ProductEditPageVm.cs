using FurnitureShop.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FurnitureShop.ViewModels.Admin.Products;

public sealed class ProductEditPageVm
{
    public ProductEditVm Form { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<ProductGalleryItemVm> Gallery { get; set; } = new();
}
