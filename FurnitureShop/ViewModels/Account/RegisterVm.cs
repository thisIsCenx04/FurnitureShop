using System.ComponentModel.DataAnnotations;

namespace FurnitureShop.ViewModels.Account;

public sealed class RegisterVm
{
    [Required, StringLength(50)]
    public string UserName { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? FullName { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";
}
