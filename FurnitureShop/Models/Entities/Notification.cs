using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Models.Entities;

[Index("CreatedAt", Name = "IX_Notifications_CreatedAt")]
[Index("UserId", "IsRead", Name = "IX_Notifications_UserId_IsRead")]
public partial class Notification
{
    [Key]
    public long Id { get; set; }

    public string? UserId { get; set; }

    [StringLength(50)]
    public string Type { get; set; } = null!;

    [StringLength(150)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string Message { get; set; } = null!;

    [StringLength(300)]
    public string? Url { get; set; }

    public bool IsRead { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User? User { get; set; }
}
