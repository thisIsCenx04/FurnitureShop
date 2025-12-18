using System.ComponentModel.DataAnnotations;

namespace FurnitureShop.ViewModels.Products;

public sealed class ProductListFilterVm
{
    public string? Category { get; set; }      // slug
    public string? Q { get; set; }             // keyword

    [Range(0, double.MaxValue)]
    public decimal? Min { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Max { get; set; }

    public string? Material { get; set; }
    public string? Style { get; set; }

    // newest | price_asc | price_desc | name_asc
    public string Sort { get; set; } = "newest";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public List<string> Categories { get; set; } = []; // multiple slugs

}
