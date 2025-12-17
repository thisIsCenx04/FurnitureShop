using System.ComponentModel.DataAnnotations;

namespace FurnitureShop.ViewModels.Account;

public sealed class LoginVm
{
    [Required]
    public string UserNameOrEmail { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; } = true;
}
