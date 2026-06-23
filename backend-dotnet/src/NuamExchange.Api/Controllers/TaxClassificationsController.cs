using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Api.Contracts.TaxClassifications;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/tax-classifications")]
[Authorize(Policy = "TaxClassificationRead")]
public sealed class TaxClassificationsController(IServiceProvider services, ITaxClassificationQueryValidator queryValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaxClassificationListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get([FromQuery] TaxClassificationListRequest request, CancellationToken cancellationToken)
    {
        var validation = queryValidator.Validate(request.ToQuery());
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });

        return await ExecuteAsync(async service => Ok(await service.GetAsync(validation.Query!, cancellationToken)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaxClassificationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => await ExecuteAsync(async service =>
        {
            var item = await service.GetByIdAsync(id, cancellationToken);
            return item is null ? NotFound(new { message = "La calificación tributaria no existe." }) : Ok(item);
        });

    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(TaxClassificationFilterOptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetFilterOptions(CancellationToken cancellationToken)
        => await ExecuteAsync(async service => Ok(await service.GetFilterOptionsAsync(cancellationToken)));

    private async Task<IActionResult> ExecuteAsync(Func<ITaxClassificationQueryService, Task<IActionResult>> action)
    {
        var service = services.GetService<ITaxClassificationQueryService>();
        if (service is null) return DatabaseUnavailable();

        try { return await action(service); }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested) { return DatabaseUnavailable(); }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
