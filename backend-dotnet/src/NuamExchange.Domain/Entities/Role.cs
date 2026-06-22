namespace NuamExchange.Domain.Entities;

public sealed class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<ApplicationUser> Users { get; } = new List<ApplicationUser>();
    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}
