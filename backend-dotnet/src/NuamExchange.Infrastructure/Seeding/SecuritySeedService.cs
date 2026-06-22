using Microsoft.EntityFrameworkCore;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.Seeding;

public interface ISecuritySeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public sealed class SecuritySeedService(NuamExchangeDbContext dbContext) : ISecuritySeedService
{
    public const string AdministratorRole = "Administrador";
    public const string TaxAnalystRole = "Analista Tributario";
    public const string SupervisorRole = "Supervisor";

    private static readonly string[] PermissionCodes =
    [
        "users.manage", "roles.view", "tax-classifications.read", "tax-classifications.write", "uploads.x-factor",
        "uploads.x-amount", "uploads.review", "reports.read", "audit.read", "backups.read"
    ];

    private static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        [AdministratorRole] = PermissionCodes,
        [TaxAnalystRole] = ["tax-classifications.read", "tax-classifications.write", "uploads.x-factor", "uploads.x-amount", "reports.read"],
        [SupervisorRole] = ["tax-classifications.read", "uploads.review", "reports.read"]
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var roleName in RolePermissions.Keys)
        {
            if (!await dbContext.Roles.AnyAsync(x => x.Name == roleName, cancellationToken))
            {
                dbContext.Roles.Add(new Role { Name = roleName, Description = $"Rol {roleName}", IsActive = true, CreatedAt = now });
            }
        }

        foreach (var code in PermissionCodes)
        {
            if (!await dbContext.Permissions.AnyAsync(x => x.Code == code, cancellationToken))
            {
                dbContext.Permissions.Add(new Permission { Code = code, Description = $"Permiso {code}" });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = await dbContext.Roles.ToDictionaryAsync(x => x.Name, cancellationToken);
        var permissions = await dbContext.Permissions.ToDictionaryAsync(x => x.Code, cancellationToken);

        foreach (var (roleName, permissionCodes) in RolePermissions)
        {
            var role = roles[roleName];
            foreach (var code in permissionCodes)
            {
                var permission = permissions[code];
                if (!await dbContext.RolePermissions.AnyAsync(x => x.RoleId == role.Id && x.PermissionId == permission.Id, cancellationToken))
                {
                    dbContext.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
