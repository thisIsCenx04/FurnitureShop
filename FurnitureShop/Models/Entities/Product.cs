using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Models.Entities;

[Index("CategoryId", Name = "IX_Products_CategoryId")]
[Index("IsActive", Name = "IX_Products_IsActive")]
[Index("Price", Name = "IX_Products_Price")]
[Index("Slug", Name = "UQ_Products_Slug", IsUnique = true)]
public partial class Product
{
    [Key]
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(250)]
    [Unicode(false)]
    public string Slug { get; set; } = null!;

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [StringLength(150)]
    public string? Material { get; set; }

    [StringLength(150)]
    public string? Dimensions { get; set; }

    [StringLength(100)]
    public string? Style { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? SalePrice { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; }

    [StringLength(500)]
    public string? MainImageUrl { get; set; }

    [StringLength(200)]
    public string? MainImagePublicId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category Category { get; set; } = null!;

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Product")]
    public virtual ProductImage? ProductImage { get; set; }
}
