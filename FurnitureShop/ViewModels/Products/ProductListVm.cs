using FurnitureShop.Helpers;

namespace FurnitureShop.ViewModels.Products;

public sealed class ProductListVm
{
    public ProductListFilterVm Filter { get; set; } = new();

    public string? SelectedCategoryName { get; set; }

    public List<ProductListItemVm> Items { get; set; } = [];

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Filter.PageSize);

    // Options
    public List<CategoryTreeHelper.FlatCategory> Categories { get; set; } = [];
    public List<string> Materials { get; set; } = [];
    public List<string> Styles { get; set; } = [];
    public List<CategoryNodeVm> CategoryTree { get; set; } = [];

}
