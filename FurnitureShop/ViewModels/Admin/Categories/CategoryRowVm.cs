namespace FurnitureShop.ViewModels.Admin.Categories;

public sealed class CategoryRowVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? ParentName { get; set; }
    public bool IsActive { get; set; }
}
