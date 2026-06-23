using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Api.Contracts.Setup;
using NuamExchange.Application.Security;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Authentication;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/setup")]
public sealed class SetupController(IServiceProvider services, IWebHostEnvironment environment, IPasswordHasher passwordHasher, IPasswordPolicy passwordPolicy) : ControllerBase
{
    [HttpPost("bootstrap-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> BootstrapAdmin([FromBody] BootstrapAdminRequest request, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (!passwordPolicy.IsValid(request.Password))
        {
            return BadRequest(new { message = "La contraseña no cumple los requisitos mínimos de seguridad." });
        }

        var dbContext = services.GetService<NuamExchangeDbContext>();
        var seedService = services.GetService<ISecuritySeedService>();
        if (dbContext is null || seedService is null)
        {
            return DatabaseUnavailable();
        }

        try
        {
            if (await dbContext.Users.AnyAsync(cancellationToken))
            {
                return Conflict(new { message = "El bootstrap inicial no está disponible porque ya existen usuarios." });
            }

            await seedService.SeedAsync(cancellationToken);
            var administratorRole = await dbContext.Roles.SingleAsync(x => x.Name == SecuritySeedService.AdministratorRole, cancellationToken);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;
            var user = new ApplicationUser
            {
                RoleId = administratorRole.Id,
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHasher.Hash(request.Password),
                JobTitle = string.IsNullOrWhiteSpace(request.JobTitle) ? null : request.JobTitle.Trim(),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = user.Id,
                AffectedEntity = "Authentication",
                AffectedRecordId = user.Id,
                Action = "BOOTSTRAP_ADMIN_CREATED",
                Detail = "Administrador inicial creado mediante bootstrap de Development.",
                OriginIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
                ActionAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(BootstrapAdmin), new { id = user.Id }, new BootstrapAdminResponse(user.Id, user.FullName, user.Email, administratorRole.Name, "Bootstrap de administrador creado."));
        }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return DatabaseUnavailable();
        }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
