using System;

namespace FurnitureShop.ViewModels.Admin.Users;

public sealed class AdminUserListVm
{
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int Total { get; set; }

    public List<Item> Items { get; set; } = new();

    public sealed class Item
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public byte Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
