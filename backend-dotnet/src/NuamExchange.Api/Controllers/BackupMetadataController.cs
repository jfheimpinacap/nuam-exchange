using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuamExchange.Api.Contracts.BackupMetadata;
using NuamExchange.Application.BackupMetadata;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/backup-metadata")]
[Authorize(Policy = "BackupMetadataRead")]
public sealed class BackupMetadataController(IServiceProvider services, IBackupMetadataQueryValidator validator, ILogger<BackupMetadataController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BackupMetadataListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get([FromQuery] BackupMetadataRequest request, CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request.ToQuery());
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service => Ok(await service.GetAsync(validation.Query!, cancellationToken)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BackupMetadataDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (id < 1) return BadRequest(new { message = "id debe ser mayor o igual a 1." });
        return await ExecuteAsync(async service =>
        {
            var backup = await service.GetByIdAsync(id, cancellationToken);
            return backup is null ? NotFound() : Ok(backup);
        });
    }

    private async Task<IActionResult> ExecuteAsync(Func<IBackupMetadataQueryService, Task<IActionResult>> action)
    {
        var service = services.GetService<IBackupMetadataQueryService>();
        if (service is null) return DatabaseUnavailable();
        try { return await action(service); }
        catch (Exception ex) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogError(ex, "Error al consultar metadatos de respaldos.");
            return DatabaseUnavailable();
        }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
