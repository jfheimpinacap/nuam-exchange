namespace NuamExchange.Application.Administration;

public sealed record AdminRoleSummaryResponse(int Id, string Name);
public sealed record AdminUserResponse(int Id, string FullName, string Email, string? JobTitle, AdminRoleSummaryResponse Role, bool IsActive, DateTime? LastAccessAt, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record PagedResponse<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);
public sealed record AdminPermissionResponse(int Id, string Code, string? Description);
public sealed record AdminRoleResponse(int Id, string Name, string? Description, bool IsActive, bool IsSystemRole, IReadOnlyCollection<AdminPermissionResponse> Permissions);
public sealed record AdminRoleDetailResponse(int Id, string Name, string? Description, bool IsActive, bool IsSystemRole, IReadOnlyCollection<AdminPermissionResponse> Permissions);
public sealed record CreateRoleCommand(string Name, string? Description, IReadOnlyCollection<int>? PermissionIds);
public sealed record UpdateRoleCommand(string Name, string? Description, bool IsActive);
public sealed record UpdateRolePermissionsCommand(IReadOnlyCollection<int>? PermissionIds);
public sealed record AdministrationResult<T>(bool Succeeded, T? Value, int StatusCode, string? Message)
{
    public static AdministrationResult<T> Ok(T value) => new(true, value, 200, null);
    public static AdministrationResult<T> Created(T value) => new(true, value, 201, null);
    public static AdministrationResult<T> Fail(int statusCode, string message) => new(false, default, statusCode, message);
}
public sealed record AdministrationResult(bool Succeeded, int StatusCode, string? Message)
{
    public static AdministrationResult NoContent() => new(true, 204, null);
    public static AdministrationResult Fail(int statusCode, string message) => new(false, statusCode, message);
}
