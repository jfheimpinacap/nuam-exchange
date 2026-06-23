using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.Administration;
using NuamExchange.Application.Security;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Authentication;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Infrastructure.Administration;

public sealed class AdministrationService(NuamExchangeDbContext dbContext, IPasswordHasher passwordHasher, IPasswordPolicy passwordPolicy) : IAdministrationService
{
    public async Task<AdministrationResult<PagedResponse<AdminUserResponse>>> GetUsersAsync(string? search, int? roleId, bool? isActive, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 100);
        var query = dbContext.Users.AsNoTracking().Include(u => u.Role).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }
        if (roleId.HasValue) query = query.Where(u => u.RoleId == roleId.Value);
        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);
        var total = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(u => u.FullName).ThenBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = users.Select(ToUserResponse).ToList();
        return AdministrationResult<PagedResponse<AdminUserResponse>>.Ok(new PagedResponse<AdminUserResponse>(items, page, pageSize, total));
    }

    public async Task<AdministrationResult<AdminUserResponse>> GetUserAsync(int id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user is null ? AdministrationResult<AdminUserResponse>.Fail(404, "El usuario indicado no existe.") : AdministrationResult<AdminUserResponse>.Ok(ToUserResponse(user));
    }

    public async Task<AdministrationResult<AdminUserResponse>> CreateUserAsync(string fullName, string email, string password, string? jobTitle, int roleId, int administratorId, string? originIp, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 150 || Clean(jobTitle)?.Length > 120) return AdministrationResult<AdminUserResponse>.Fail(400, "Los datos del usuario no cumplen las validaciones requeridas.");
        var normalizedEmail = NormalizeEmail(email);
        var role = await dbContext.Roles.SingleOrDefaultAsync(r => r.Id == roleId && r.IsActive, cancellationToken);
        if (role is null) return AdministrationResult<AdminUserResponse>.Fail(400, "El rol indicado no existe o está inactivo.");
        if (!passwordPolicy.IsValid(password)) return AdministrationResult<AdminUserResponse>.Fail(400, "La contraseña no cumple los requisitos mínimos de seguridad.");
        if (await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken)) return AdministrationResult<AdminUserResponse>.Fail(409, "El correo ya está registrado.");
        var now = DateTime.UtcNow;
        var user = new ApplicationUser { FullName = fullName.Trim(), Email = normalizedEmail, PasswordHash = passwordHasher.Hash(password), JobTitle = Clean(jobTitle), RoleId = roleId, IsActive = true, CreatedAt = now, UpdatedAt = now };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        user.Role = role;
        await AuditAsync(administratorId, user.Id, "USER_CREATED", $"Usuario creado por administración. RolId: {roleId}.", originIp, cancellationToken);
        return AdministrationResult<AdminUserResponse>.Created(ToUserResponse(user));
    }

    public async Task<AdministrationResult<AdminUserResponse>> UpdateUserAsync(int id, string fullName, string email, string? jobTitle, int roleId, bool isActive, int administratorId, string? originIp, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 150 || Clean(jobTitle)?.Length > 120) return AdministrationResult<AdminUserResponse>.Fail(400, "Los datos del usuario no cumplen las validaciones requeridas.");
        var user = await dbContext.Users.Include(u => u.Role).SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return AdministrationResult<AdminUserResponse>.Fail(404, "El usuario indicado no existe.");
        var role = await dbContext.Roles.SingleOrDefaultAsync(r => r.Id == roleId && r.IsActive, cancellationToken);
        if (role is null) return AdministrationResult<AdminUserResponse>.Fail(400, "El rol indicado no existe o está inactivo.");
        var normalizedEmail = NormalizeEmail(email);
        if (await dbContext.Users.AnyAsync(u => u.Id != id && u.Email == normalizedEmail, cancellationToken)) return AdministrationResult<AdminUserResponse>.Fail(409, "El correo ya está registrado.");
        if (id == administratorId && !isActive) return AdministrationResult<AdminUserResponse>.Fail(409, "No es posible desactivar el usuario autenticado.");
        if (id == administratorId && user.RoleId != roleId) return AdministrationResult<AdminUserResponse>.Fail(409, "No es posible modificar el rol del usuario autenticado.");
        if (user.Role.Name == SecuritySeedService.AdministratorRole && (!isActive || role.Name != SecuritySeedService.AdministratorRole) && await ActiveAdministratorsAsync(cancellationToken) <= 1)
        {
            return AdministrationResult<AdminUserResponse>.Fail(409, "No es posible desactivar el último Administrador activo.");
        }
        var changed = new List<string>();
        SetIfChanged(changed, "fullName", user.FullName, fullName.Trim(), v => user.FullName = v);
        SetIfChanged(changed, "email", user.Email, normalizedEmail, v => user.Email = v);
        SetIfChanged(changed, "jobTitle", user.JobTitle, Clean(jobTitle), v => user.JobTitle = v);
        if (user.RoleId != roleId) { changed.Add("roleId"); user.RoleId = roleId; user.Role = role; }
        if (user.IsActive != isActive) { changed.Add("isActive"); user.IsActive = isActive; }
        if (changed.Count > 0) { user.UpdatedAt = DateTime.UtcNow; await dbContext.SaveChangesAsync(cancellationToken); await AuditAsync(administratorId, user.Id, "USER_UPDATED", $"Usuario actualizado por administración. Campos: {string.Join(", ", changed)}.", originIp, cancellationToken); }
        return AdministrationResult<AdminUserResponse>.Ok(ToUserResponse(user));
    }

    public async Task<AdministrationResult> ResetPasswordAsync(int id, string newPassword, int administratorId, string? originIp, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null) return AdministrationResult.Fail(404, "El usuario indicado no existe.");
        if (!passwordPolicy.IsValid(newPassword)) return AdministrationResult.Fail(400, "La contraseña no cumple los requisitos mínimos de seguridad.");
        user.PasswordHash = passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(administratorId, user.Id, "USER_PASSWORD_RESET", "Contraseña restablecida por administración.", originIp, cancellationToken);
        return AdministrationResult.NoContent();
    }

    public async Task<IReadOnlyCollection<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken) => await dbContext.Roles.AsNoTracking().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).OrderBy(r => r.Name).Select(r => new AdminRoleResponse(r.Id, r.Name, r.Description, r.IsActive, r.RolePermissions.Select(rp => rp.Permission.Code).Distinct().OrderBy(p => p).ToList())).ToListAsync(cancellationToken);
    public async Task<IReadOnlyCollection<AdminPermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken) => await dbContext.Permissions.AsNoTracking().OrderBy(p => p.Code).Select(p => new AdminPermissionResponse(p.Id, p.Code, p.Description)).ToListAsync(cancellationToken);

    private async Task<int> ActiveAdministratorsAsync(CancellationToken ct) => await dbContext.Users.CountAsync(u => u.IsActive && u.Role.Name == SecuritySeedService.AdministratorRole, ct);
    private async Task AuditAsync(int actorId, int affectedId, string action, string detail, string? originIp, CancellationToken ct) { dbContext.AuditLogs.Add(new AuditLog { UserId = actorId, AffectedEntity = "Usuario", AffectedRecordId = affectedId, Action = action, Detail = detail, OriginIp = originIp, ActionAt = DateTime.UtcNow }); await dbContext.SaveChangesAsync(ct); }
    private static AdminUserResponse ToUserResponse(ApplicationUser u) => new(u.Id, u.FullName, u.Email, u.JobTitle, new AdminRoleSummaryResponse(u.Role.Id, u.Role.Name), u.IsActive, u.LastAccessAt, u.CreatedAt, u.UpdatedAt);
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void SetIfChanged<T>(ICollection<string> changed, string name, T current, T next, Action<T> setter) { if (!EqualityComparer<T>.Default.Equals(current, next)) { changed.Add(name); setter(next); } }
}
