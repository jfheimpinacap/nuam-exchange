using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/bulk-loads")]
[Authorize(Policy = "TaxClassificationRead")]
public sealed class BulkLoadsController(IServiceProvider services, IBulkLoadQueryValidator validator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BulkLoadSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get([FromQuery] BulkLoadQuery request, CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request);
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service => Ok(await service.GetAsync(validation.Query!, cancellationToken)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BulkLoadSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => await ExecuteAsync(async service =>
        {
            var item = await service.GetByIdAsync(id, cancellationToken);
            return item is null ? NotFound(new { message = "La carga masiva no existe." }) : Ok(item);
        });

    [HttpGet("{id:int}/details")]
    [ProducesResponseType(typeof(PagedResult<BulkLoadDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetails(int id, [FromQuery] BulkLoadDetailQuery request, CancellationToken cancellationToken)
    {
        var validation = validator.ValidateDetails(request);
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service =>
        {
            var result = await service.GetDetailsAsync(id, validation.Query!, cancellationToken);
            return result is null ? NotFound(new { message = "La carga masiva no existe." }) : Ok(result);
        });
    }

    [HttpGet("{id:int}/errors")]
    [ProducesResponseType(typeof(PagedResult<BulkLoadErrorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetErrors(int id, [FromQuery] BulkLoadErrorQuery request, CancellationToken cancellationToken)
    {
        var validation = validator.ValidateErrors(request);
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service =>
        {
            var result = await service.GetErrorsAsync(id, validation.Query!, cancellationToken);
            return result is null ? NotFound(new { message = "La carga masiva no existe." }) : Ok(result);
        });
    }

    private async Task<IActionResult> ExecuteAsync(Func<IBulkLoadQueryService, Task<IActionResult>> action)
    {
        var service = services.GetService<IBulkLoadQueryService>();
        if (service is null) return DatabaseUnavailable();
        try { return await action(service); }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested) { return DatabaseUnavailable(); }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
