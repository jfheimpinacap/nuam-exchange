namespace NuamExchange.Domain.Entities;

public sealed class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; } = new List<RolePermission>();
}
