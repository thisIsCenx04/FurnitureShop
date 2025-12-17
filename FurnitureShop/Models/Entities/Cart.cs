using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Models.Entities;

[Index("UserId", Name = "IX_Carts_UserId")]
[Index("CartKey", Name = "UQ_Carts_CartKey", IsUnique = true)]
public partial class Cart
{
    [Key]
    public long Id { get; set; }

    public string? UserId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string CartKey { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Cart")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [ForeignKey("UserId")]
    [InverseProperty("Carts")]
    public virtual User? User { get; set; }
}
