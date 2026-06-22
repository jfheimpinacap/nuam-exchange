using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Api.Contracts.Auth;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Authentication;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IServiceProvider services, JwtConfigurationState jwtState, IPasswordHasher passwordHasher, IAccessTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!jwtState.IsConfigured)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La autenticación no está configurada para este entorno." });
        }

        var dbContext = services.GetService<NuamExchangeDbContext>();
        if (dbContext is null)
        {
            return DatabaseUnavailable();
        }

        try
        {
            var normalizedEmail = NormalizeEmail(request.Email);
            var user = await dbContext.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
            var valid = user is not null && user.IsActive && user.Role.IsActive && passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!valid)
            {
                await AuditAsync(dbContext, user?.Id, "LOGIN_FAILED", "Intento de inicio de sesión fallido.", cancellationToken);
                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            user!.LastAccessAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            var token = tokenService.CreateToken(new AccessTokenUser(user.Id, user.FullName, user.Email, user.RoleId, user.Role.Name));
            await AuditAsync(dbContext, user.Id, "LOGIN_SUCCESS", "Inicio de sesión exitoso.", cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new LoginResponse(token.Token, "Bearer", token.ExpiresAt, new AuthUserResponse(user.Id, user.FullName, user.Email, user.Role.Name)));
        }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return DatabaseUnavailable();
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var dbContext = services.GetService<NuamExchangeDbContext>();
        if (dbContext is null) return DatabaseUnavailable();
        if (!TryGetUserId(out var userId)) return Unauthorized();

        try
        {
            var user = await dbContext.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
            if (user is null) return Unauthorized();
            return Ok(new MeResponse(user.Id, user.FullName, user.Email, user.JobTitle, user.Role.Name, user.IsActive));
        }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return DatabaseUnavailable();
        }
    }

    [HttpGet("permissions")]
    [Authorize]
    public async Task<IActionResult> Permissions(CancellationToken cancellationToken)
    {
        var dbContext = services.GetService<NuamExchangeDbContext>();
        if (dbContext is null) return DatabaseUnavailable();
        if (!TryGetUserId(out var userId)) return Unauthorized();

        try
        {
            var permissions = await dbContext.Users
                .Where(u => u.Id == userId && u.IsActive && u.Role.IsActive)
                .SelectMany(u => u.Role.RolePermissions.Select(rp => rp.Permission.Code))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            return Ok(new PermissionsResponse(permissions));
        }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return DatabaseUnavailable();
        }
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(value, out userId);
    }

    private async Task AuditAsync(NuamExchangeDbContext dbContext, int? userId, string action, string detail, CancellationToken cancellationToken)
    {
        dbContext.AuditLogs.Add(new AuditLog { UserId = userId, AffectedEntity = "Authentication", Action = action, Detail = detail, OriginIp = HttpContext.Connection.RemoteIpAddress?.ToString(), ActionAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
