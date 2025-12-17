using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Models.Entities;

[Index("ChangedAt", Name = "IX_OrderStatusHistories_ChangedAt")]
[Index("OrderId", Name = "IX_OrderStatusHistories_OrderId")]
public partial class OrderStatusHistory
{
    [Key]
    public long Id { get; set; }

    public long OrderId { get; set; }

    public byte FromStatus { get; set; }

    public byte ToStatus { get; set; }

    [StringLength(450)]
    public string? ChangedByUserId { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    [Precision(0)]
    public DateTime ChangedAt { get; set; }

    [ForeignKey("ChangedByUserId")]
    [InverseProperty("OrderStatusHistories")]
    public virtual User? ChangedByUser { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderStatusHistories")]
    public virtual Order Order { get; set; } = null!;
}
