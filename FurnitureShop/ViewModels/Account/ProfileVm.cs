using System.ComponentModel.DataAnnotations;

namespace FurnitureShop.ViewModels.Account;

public sealed class ProfileVm
{
    public string Id { get; set; } = "";

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
}
