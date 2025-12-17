using FurnitureShop.Services.Auth;
using FurnitureShop.ViewModels.Account;
using FurnitureShop.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FurnitureShop.Controllers;

public sealed class AccountController : Controller
{
    private readonly IUserService _users;

    public AccountController(IUserService users) => _users = users;

    // GET: /account/login?returnUrl=...
    [HttpGet("/account/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginVm());
    }

    // POST: /account/login
    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByLoginAsync(vm.UserNameOrEmail);
        if (user == null || user.IsActive == false)
        {
            ModelState.AddModelError("", "Tài khoản không tồn tại hoặc đã bị khóa.");
            return View(vm);
        }

        var ok = await _users.VerifyPasswordAsync(user, vm.Password);
        if (!ok)
        {
            ModelState.AddModelError("", "Sai mật khẩu.");
            return View(vm);
        }

        // Claims bắt buộc theo yêu cầu: uid, role
        var roleName = (user.Role == 1) ? "Admin" : "Customer";

        var claims = new List<Claim>
        {
            new("uid", user.Id),
            new("role", roleName),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Role, roleName),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = vm.RememberMe,
                RedirectUri = returnUrl
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Ưu tiên returnUrl nếu có (và hợp lệ)
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        // Redirect theo role
        if (roleName == "Admin")
        {
            return Redirect("/Admin");
            // return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // Customer / user thường
        return RedirectToAction("Index", "Home", new { area = "" });

    }

    // GET: /account/register
    [HttpGet("/account/register")]
    public IActionResult Register() => View(new RegisterVm());

    // POST: /account/register
    [HttpPost("/account/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            UserName = vm.UserName,
            Email = vm.Email,
            Phone = vm.Phone,
            FullName = vm.FullName,
            Address = vm.Address,
            Role = 0,        // Customer
            IsActive = true
        };

        var (ok, err) = await _users.RegisterAsync(user, vm.Password);
        if (!ok)
        {
            ModelState.AddModelError("", err);
            return View(vm);
        }

        return RedirectToAction(nameof(Login));
    }

    // POST: /account/logout
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    // GET: /account/profile
    [Authorize]
    [HttpGet("/account/profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(Login));

        var user = await _users.FindByIdAsync(userId);
        if (user == null) return RedirectToAction(nameof(Login));

        var vm = new ProfileVm
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Phone = user.Phone,
            FullName = user.FullName,
            Address = user.Address
        };

        return View(vm);
    }

    // POST: /account/profile
    [Authorize]
    [HttpPost("/account/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var userId = User.FindFirst("uid")?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return RedirectToAction(nameof(Login));

        var (ok, err) = await _users.UpdateProfileAsync(userId, vm.FullName, vm.Phone, vm.Address);
        if (!ok)
        {
            ModelState.AddModelError("", err);
            return View(vm);
        }

        ViewBag.Success = "Cập nhật thông tin thành công.";
        return View(vm);
    }

    [HttpGet("/account/access-denied")]
    public IActionResult AccessDenied() => View();
}
