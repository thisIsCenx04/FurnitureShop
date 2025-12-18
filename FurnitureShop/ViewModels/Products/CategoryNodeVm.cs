namespace FurnitureShop.ViewModels.Products;

public sealed class CategoryNodeVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int? ParentId { get; set; }

    public List<CategoryNodeVm> Children { get; set; } = [];
}
