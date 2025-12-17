using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Models.Entities;

[Index("CreatedAt", Name = "IX_Orders_CreatedAt")]
[Index("Status", Name = "IX_Orders_Status")]
[Index("UserId", Name = "IX_Orders_UserId")]
[Index("OrderCode", Name = "UQ_Orders_OrderCode", IsUnique = true)]
public partial class Order
{
    [Key]
    public long Id { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string OrderCode { get; set; } = null!;

    public string? UserId { get; set; }

    [StringLength(150)]
    public string CustomerName { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Phone { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(300)]
    public string Address { get; set; } = null!;

    [StringLength(500)]
    public string? Note { get; set; }

    public byte PaymentMethod { get; set; }

    public byte Status { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ShippingFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User? User { get; set; }
}
