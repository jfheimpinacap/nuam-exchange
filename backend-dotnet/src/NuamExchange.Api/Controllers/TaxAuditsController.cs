using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuamExchange.Api.Contracts.TaxAudits;
using NuamExchange.Application.TaxAudits;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/tax-audits")]
[Authorize(Policy = "TaxClassificationRead")]
public sealed class TaxAuditsController(IServiceProvider services, ITaxAuditQueryValidator validator, ILogger<TaxAuditsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaxAuditListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get([FromQuery] TaxAuditRequest request, CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request.ToQuery());
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service => Ok(await service.GetAsync(validation.Query!, cancellationToken)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaxAuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => await ExecuteAsync(async service =>
        {
            var audit = await service.GetByIdAsync(id, cancellationToken);
            return audit is null ? NotFound() : Ok(audit);
        });

    private async Task<IActionResult> ExecuteAsync(Func<ITaxAuditQueryService, Task<IActionResult>> action)
    {
        var service = services.GetService<ITaxAuditQueryService>();
        if (service is null) return DatabaseUnavailable();
        try { return await action(service); }
        catch (Exception ex) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogError(ex, "Error al consultar auditoría tributaria.");
            return DatabaseUnavailable();
        }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
