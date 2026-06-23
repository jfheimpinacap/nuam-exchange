using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuamExchange.Api.Contracts.Administration;
using NuamExchange.Application.Administration;

namespace NuamExchange.Api.Controllers.Administration;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdministratorOnly")]
public sealed class AdminController(IServiceProvider services) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int? roleId, [FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => await ExecuteAsync(async service => ToActionResult(await service.GetUsersAsync(search, roleId, isActive, page, pageSize, cancellationToken)));

    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        => await ExecuteAsync(async service => ToActionResult(await service.GetUserAsync(id, cancellationToken)));

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var administratorId)) return Unauthorized();
        return await ExecuteAsync(async service =>
        {
            var result = await service.CreateUserAsync(request.FullName, request.Email, request.Password, request.JobTitle, request.RoleId, administratorId, OriginIp(), cancellationToken);
            return result.Succeeded ? CreatedAtAction(nameof(GetUser), new { id = result.Value!.Id }, result.Value) : Error(result.StatusCode, result.Message!);
        });
    }

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var administratorId)) return Unauthorized();
        return await ExecuteAsync(async service => ToActionResult(await service.UpdateUserAsync(id, request.FullName, request.Email, request.JobTitle, request.RoleId, request.IsActive, administratorId, OriginIp(), cancellationToken)));
    }

    [HttpPost("users/{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetAdminUserPasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var administratorId)) return Unauthorized();
        return await ExecuteAsync(async service =>
        {
            var result = await service.ResetPasswordAsync(id, request.NewPassword, administratorId, OriginIp(), cancellationToken);
            return result.Succeeded ? NoContent() : Error(result.StatusCode, result.Message!);
        });
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
        => await ExecuteAsync(async service => Ok(await service.GetRolesAsync(cancellationToken)));

    [HttpGet("roles/{id:int}")]
    public async Task<IActionResult> GetRole(int id, CancellationToken cancellationToken)
        => await ExecuteAsync(async service => ToActionResult(await service.GetRoleAsync(id, cancellationToken)));

    [HttpPost("roles")]
    [ProducesResponseType(typeof(AdminRoleDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateAdminRoleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var administratorId)) return Unauthorized();
        return await ExecuteAsync(async service =>
        {
            var result = await service.CreateRoleAsync(new CreateRoleCommand(request.Name, request.Description, request.PermissionIds), administratorId, OriginIp(), cancellationToken);
            return result.Succeeded ? CreatedAtAction(nameof(GetRole), new { id = result.Value!.Id }, result.Value) : Error(result.StatusCode, result.Message!);
        });
    }

    [HttpPut("roles/{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateAdminRoleRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var administratorId)) return Unauthorized();
        return await ExecuteAsync(async service => ToActionResult(await service.UpdateRoleAsync(id, new UpdateRoleCommand(request.Name, request.Description, request.IsActive), administratorId, OriginIp(), cancellationToken)));
    }

    [HttpPut("roles/{id:int}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateAdminRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var administratorId)) return Unauthorized();
        return await ExecuteAsync(async service => ToActionResult(await service.UpdateRolePermissionsAsync(id, new UpdateRolePermissionsCommand(request.PermissionIds), administratorId, OriginIp(), cancellationToken)));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
        => await ExecuteAsync(async service => Ok(await service.GetPermissionsAsync(cancellationToken)));

    private async Task<IActionResult> ExecuteAsync(Func<IAdministrationService, Task<IActionResult>> action)
    {
        var service = services.GetService<IAdministrationService>();
        if (service is null) return DatabaseUnavailable();
        try { return await action(service); }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested) { return DatabaseUnavailable(); }
    }

    private IActionResult ToActionResult<T>(AdministrationResult<T> result) => result.Succeeded ? Ok(result.Value) : Error(result.StatusCode, result.Message!);
    private ObjectResult Error(int statusCode, string message) => StatusCode(statusCode, new { message });
    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
    private string? OriginIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value, out userId);
}
