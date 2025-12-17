using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Models.Entities;

[Index("ProductId", Name = "IX_ProductImages_ProductId")]
public partial class ProductImage
{
    [Key]
    public long Id { get; set; }

    public int ProductId { get; set; }

    [StringLength(500)]
    public string ImageUrl { get; set; } = null!;

    [StringLength(200)]
    public string PublicId { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductImage")]
    public virtual Product Product { get; set; } = null!;
}
