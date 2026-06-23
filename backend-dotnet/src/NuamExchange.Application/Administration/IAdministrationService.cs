namespace NuamExchange.Application.Administration;

public interface IAdministrationService
{
    Task<AdministrationResult<PagedResponse<AdminUserResponse>>> GetUsersAsync(string? search, int? roleId, bool? isActive, int page, int pageSize, CancellationToken cancellationToken);
    Task<AdministrationResult<AdminUserResponse>> GetUserAsync(int id, CancellationToken cancellationToken);
    Task<AdministrationResult<AdminUserResponse>> CreateUserAsync(string fullName, string email, string password, string? jobTitle, int roleId, int administratorId, string? originIp, CancellationToken cancellationToken);
    Task<AdministrationResult<AdminUserResponse>> UpdateUserAsync(int id, string fullName, string email, string? jobTitle, int roleId, bool isActive, int administratorId, string? originIp, CancellationToken cancellationToken);
    Task<AdministrationResult> ResetPasswordAsync(int id, string newPassword, int administratorId, string? originIp, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminRoleResponse>> GetRolesAsync(CancellationToken cancellationToken);
    Task<AdministrationResult<AdminRoleDetailResponse>> GetRoleAsync(int id, CancellationToken cancellationToken);
    Task<AdministrationResult<AdminRoleDetailResponse>> CreateRoleAsync(CreateRoleCommand command, int administratorId, string? originIp, CancellationToken cancellationToken);
    Task<AdministrationResult<AdminRoleDetailResponse>> UpdateRoleAsync(int id, UpdateRoleCommand command, int administratorId, string? originIp, CancellationToken cancellationToken);
    Task<AdministrationResult<AdminRoleDetailResponse>> UpdateRolePermissionsAsync(int id, UpdateRolePermissionsCommand command, int administratorId, string? originIp, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdminPermissionResponse>> GetPermissionsAsync(CancellationToken cancellationToken);
}
