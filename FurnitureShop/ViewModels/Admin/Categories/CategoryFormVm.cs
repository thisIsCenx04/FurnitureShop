using System.ComponentModel.DataAnnotations;

namespace FurnitureShop.ViewModels.Admin.Categories;

public sealed class CategoryFormVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên không được để trống")]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Slug không được để trống")]
    [StringLength(200)]
    public string Slug { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }

    public bool IsActive { get; set; } = true;

    // dropdown parent
    public List<ParentOption> ParentOptions { get; set; } = new();

    public sealed class ParentOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
    }
}
