using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.Administration;
using NuamExchange.Application.Security;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Authentication;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Infrastructure.Administration;

public sealed class AdministrationService(NuamExchangeDbContext dbContext, IPasswordHasher passwordHasher, IPasswordPolicy passwordPolicy, IRoleManagementPolicy rolePolicy) : IAdministrationService
{
    public async Task<AdministrationResult<PagedResponse<AdminUserResponse>>> GetUsersAsync(string? search, int? roleId, bool? isActive, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 100);
        var query = dbContext.Users.AsNoTracking().Include(u => u.Role).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim().ToLowerInvariant(); query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term)); }
        if (roleId.HasValue) query = query.Where(u => u.RoleId == roleId.Value);
        if (isActive.HasValue) query = query.Where(u => u.IsActive == isActive.Value);
        var total = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(u => u.FullName).ThenBy(u => u.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return AdministrationResult<PagedResponse<AdminUserResponse>>.Ok(new PagedResponse<AdminUserResponse>(users.Select(ToUserResponse).ToList(), page, pageSize, total));
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
        await AuditAsync(administratorId, "Usuario", user.Id, "USER_CREATED", $"Usuario creado por administración. RolId: {roleId}.", originIp, cancellationToken);
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
        if (user.Role.Name == SecuritySeedService.AdministratorRole && (!isActive || role.Name != SecuritySeedService.AdministratorRole) && await ActiveAdministratorsAsync(cancellationToken) <= 1) return AdministrationResult<AdminUserResponse>.Fail(409, "No es posible desactivar el último Administrador activo.");
        var changed = new List<string>();
        SetIfChanged(changed, "fullName", user.FullName, fullName.Trim(), v => user.FullName = v);
        SetIfChanged(changed, "email", user.Email, normalizedEmail, v => user.Email = v);
        SetIfChanged(changed, "jobTitle", user.JobTitle, Clean(jobTitle), v => user.JobTitle = v);
        if (user.RoleId != roleId) { changed.Add("roleId"); user.RoleId = roleId; user.Role = role; }
        if (user.IsActive != isActive) { changed.Add("isActive"); user.IsActive = isActive; }
        if (changed.Count > 0) { user.UpdatedAt = DateTime.UtcNow; await dbContext.SaveChangesAsync(cancellationToken); await AuditAsync(administratorId, "Usuario", user.Id, "USER_UPDATED", $"Usuario actualizado por administración. Campos: {string.Join(", ", changed)}.", originIp, cancellationToken); }
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
        await AuditAsync(administratorId, "Usuario", user.Id, "USER_PASSWORD_RESET", "Contraseña restablecida por administración.", originIp, cancellationToken);
        return AdministrationResult.NoContent();
    }

    public async Task<IReadOnlyCollection<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken) => (await RoleQuery().OrderBy(r => r.Name).ToListAsync(cancellationToken)).Select(ToRoleResponse).ToList();

    public async Task<AdministrationResult<AdminRoleDetailResponse>> GetRoleAsync(int id, CancellationToken cancellationToken)
    {
        var role = await RoleQuery().SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        return role is null ? AdministrationResult<AdminRoleDetailResponse>.Fail(404, "El rol indicado no existe.") : AdministrationResult<AdminRoleDetailResponse>.Ok(ToRoleDetailResponse(role));
    }

    public async Task<AdministrationResult<AdminRoleDetailResponse>> CreateRoleAsync(CreateRoleCommand command, int administratorId, string? originIp, CancellationToken cancellationToken)
    {
        var nameValidation = rolePolicy.ValidateCustomRoleName(command.Name);
        if (!nameValidation.Succeeded) return AdministrationResult<AdminRoleDetailResponse>.Fail(NameValidationFailureStatus(nameValidation), nameValidation.Message!);
        var description = Clean(command.Description);
        if (description?.Length > 250) return AdministrationResult<AdminRoleDetailResponse>.Fail(400, "La descripción del rol no puede superar 250 caracteres.");
        var permissionValidation = await ValidatePermissionIdsAsync(command.PermissionIds, cancellationToken);
        if (!permissionValidation.Succeeded) return AdministrationResult<AdminRoleDetailResponse>.Fail(permissionValidation.StatusCode, permissionValidation.Message!);
        if (await dbContext.Roles.AnyAsync(r => r.Name.ToLower() == nameValidation.NormalizedName!.ToLower(), cancellationToken)) return AdministrationResult<AdminRoleDetailResponse>.Fail(409, "Ya existe un rol con el nombre indicado.");
        var now = DateTime.UtcNow;
        var role = new Role { Name = nameValidation.NormalizedName!, Description = description, IsActive = true, CreatedAt = now };
        foreach (var permissionId in permissionValidation.PermissionIds) role.RolePermissions.Add(new RolePermission { PermissionId = permissionId });
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(administratorId, "Rol", role.Id, "ROLE_CREATED", $"Rol '{role.Name}' creado con {permissionValidation.PermissionIds.Count} permisos.", originIp, cancellationToken);
        return AdministrationResult<AdminRoleDetailResponse>.Created(ToRoleDetailResponse((await RoleQuery().SingleAsync(r => r.Id == role.Id, cancellationToken))));
    }

    public async Task<AdministrationResult<AdminRoleDetailResponse>> UpdateRoleAsync(int id, UpdateRoleCommand command, int administratorId, string? originIp, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role is null) return AdministrationResult<AdminRoleDetailResponse>.Fail(404, "El rol indicado no existe.");
        if (rolePolicy.IsSystemRole(role.Name)) return AdministrationResult<AdminRoleDetailResponse>.Fail(409, rolePolicy.ProtectedRoleMessage(role.Name));
        var nameValidation = rolePolicy.ValidateCustomRoleName(command.Name);
        if (!nameValidation.Succeeded) return AdministrationResult<AdminRoleDetailResponse>.Fail(NameValidationFailureStatus(nameValidation), nameValidation.Message!);
        var description = Clean(command.Description);
        if (description?.Length > 250) return AdministrationResult<AdminRoleDetailResponse>.Fail(400, "La descripción del rol no puede superar 250 caracteres.");
        if (await dbContext.Roles.AnyAsync(r => r.Id != id && r.Name.ToLower() == nameValidation.NormalizedName!.ToLower(), cancellationToken)) return AdministrationResult<AdminRoleDetailResponse>.Fail(409, "Ya existe un rol con el nombre indicado.");
        if (role.IsActive && !command.IsActive && await dbContext.Users.AnyAsync(u => u.RoleId == id && u.IsActive, cancellationToken)) return AdministrationResult<AdminRoleDetailResponse>.Fail(409, "No es posible desactivar un rol que tiene usuarios activos asignados.");
        var changed = new List<string>();
        SetIfChanged(changed, "name", role.Name, nameValidation.NormalizedName!, v => role.Name = v);
        SetIfChanged(changed, "description", role.Description, description, v => role.Description = v);
        if (role.IsActive != command.IsActive) { changed.Add("isActive"); role.IsActive = command.IsActive; }
        if (changed.Count > 0) { await dbContext.SaveChangesAsync(cancellationToken); await AuditAsync(administratorId, "Rol", role.Id, "ROLE_UPDATED", $"Rol actualizado. Campos: {string.Join(", ", changed)}.", originIp, cancellationToken); }
        return AdministrationResult<AdminRoleDetailResponse>.Ok(ToRoleDetailResponse(await RoleQuery().SingleAsync(r => r.Id == id, cancellationToken)));
    }

    public async Task<AdministrationResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(int id, UpdateRolePermissionsCommand command, int administratorId, string? originIp, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.Include(r => r.RolePermissions).SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (role is null) return AdministrationResult<AdminRoleDetailResponse>.Fail(404, "El rol indicado no existe.");
        if (rolePolicy.IsSystemRole(role.Name)) return AdministrationResult<AdminRoleDetailResponse>.Fail(409, rolePolicy.ProtectedRoleMessage(role.Name));
        var permissionValidation = await ValidatePermissionIdsAsync(command.PermissionIds, cancellationToken);
        if (!permissionValidation.Succeeded) return AdministrationResult<AdminRoleDetailResponse>.Fail(permissionValidation.StatusCode, permissionValidation.Message!);
        var previousCount = role.RolePermissions.Select(rp => rp.PermissionId).Distinct().Count();
        var currentRolePermissions = role.RolePermissions.ToList();
        dbContext.RolePermissions.RemoveRange(currentRolePermissions);
        role.RolePermissions.Clear();
        foreach (var permissionId in permissionValidation.PermissionIds) role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync(administratorId, "Rol", role.Id, "ROLE_PERMISSIONS_UPDATED", $"Permisos del rol actualizados. Cantidad anterior: {previousCount}. Cantidad nueva: {permissionValidation.PermissionIds.Count}.", originIp, cancellationToken);
        return AdministrationResult<AdminRoleDetailResponse>.Ok(ToRoleDetailResponse(await RoleQuery().SingleAsync(r => r.Id == id, cancellationToken)));
    }

    public async Task<IReadOnlyCollection<AdminPermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken) => await dbContext.Permissions.AsNoTracking().OrderBy(p => p.Code).Select(p => new AdminPermissionResponse(p.Id, p.Code, p.Description)).Distinct().ToListAsync(cancellationToken);

    private IQueryable<Role> RoleQuery() => dbContext.Roles.AsNoTracking().Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission);
    private async Task<int> ActiveAdministratorsAsync(CancellationToken ct) => await dbContext.Users.CountAsync(u => u.IsActive && u.Role.Name == SecuritySeedService.AdministratorRole, ct);
    private async Task AuditAsync(int actorId, string entity, int affectedId, string action, string detail, string? originIp, CancellationToken ct) { dbContext.AuditLogs.Add(new AuditLog { UserId = actorId, AffectedEntity = entity, AffectedRecordId = affectedId, Action = action, Detail = detail, OriginIp = originIp, ActionAt = DateTime.UtcNow }); await dbContext.SaveChangesAsync(ct); }
    private async Task<PermissionIdsValidationResult> ValidatePermissionIdsAsync(IReadOnlyCollection<int>? permissionIds, CancellationToken ct)
    {
        if (permissionIds is null || permissionIds.Count == 0) return PermissionIdsValidationResult.Fail(400, "Debe indicar al menos un permiso.");
        if (permissionIds.Distinct().Count() != permissionIds.Count) return PermissionIdsValidationResult.Fail(400, "La lista de permisos no puede contener identificadores repetidos.");
        var orderedIds = permissionIds.OrderBy(id => id).ToList();
        var existingIds = await dbContext.Permissions.Where(p => orderedIds.Contains(p.Id)).Select(p => p.Id).ToListAsync(ct);
        if (existingIds.Count != orderedIds.Count) return PermissionIdsValidationResult.Fail(400, "Uno o más permisos indicados no existen.");
        return PermissionIdsValidationResult.Ok(orderedIds);
    }
    private AdminRoleResponse ToRoleResponse(Role r) => new(r.Id, r.Name, r.Description, r.IsActive, rolePolicy.IsSystemRole(r.Name), ToPermissionResponses(r));
    private AdminRoleDetailResponse ToRoleDetailResponse(Role r) => new(r.Id, r.Name, r.Description, r.IsActive, rolePolicy.IsSystemRole(r.Name), ToPermissionResponses(r));
    private static IReadOnlyCollection<AdminPermissionResponse> ToPermissionResponses(Role r) => r.RolePermissions.Select(rp => rp.Permission).GroupBy(p => p.Id).Select(g => g.First()).OrderBy(p => p.Code).Select(p => new AdminPermissionResponse(p.Id, p.Code, p.Description)).ToList();
    private static int NameValidationFailureStatus(RoleNameValidationResult result) => result.Message?.Contains("reservado", StringComparison.OrdinalIgnoreCase) == true ? 409 : 400;
    private static AdminUserResponse ToUserResponse(ApplicationUser u) => new(u.Id, u.FullName, u.Email, u.JobTitle, new AdminRoleSummaryResponse(u.Role.Id, u.Role.Name), u.IsActive, u.LastAccessAt, u.CreatedAt, u.UpdatedAt);
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void SetIfChanged<T>(ICollection<string> changed, string name, T current, T next, Action<T> setter) { if (!EqualityComparer<T>.Default.Equals(current, next)) { changed.Add(name); setter(next); } }
    private sealed record PermissionIdsValidationResult(bool Succeeded, IReadOnlyCollection<int> PermissionIds, int StatusCode, string? Message) { public static PermissionIdsValidationResult Ok(IReadOnlyCollection<int> ids) => new(true, ids, 200, null); public static PermissionIdsValidationResult Fail(int statusCode, string message) => new(false, [], statusCode, message); }
}
