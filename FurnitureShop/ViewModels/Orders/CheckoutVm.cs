using System.ComponentModel.DataAnnotations;
using FurnitureShop.ViewModels.Cart;

namespace FurnitureShop.ViewModels.Orders;

public sealed class CheckoutVm
{
    public CartSummaryVm Cart { get; set; } = new();

    [Required, StringLength(100)]
    public string CustomerName { get; set; } = "";

    [Required, StringLength(20)]
    public string Phone { get; set; } = "";

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [Required, StringLength(250)]
    public string Address { get; set; } = "";

    [StringLength(500)]
    public string? Note { get; set; }
}
