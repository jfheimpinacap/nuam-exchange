using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Api.Contracts.TaxReports;
using NuamExchange.Application.TaxReports;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/tax-reports")]
[Authorize(Policy = "TaxClassificationRead")]
public sealed class TaxReportsController(IServiceProvider services, ITaxReportQueryValidator validator) : ControllerBase
{
    [HttpGet("tax-classifications")]
    [ProducesResponseType(typeof(TaxClassificationReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetTaxClassifications([FromQuery] TaxClassificationReportRequest request, CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request.ToQuery());
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service => Ok(await service.GetTaxClassificationsAsync(validation.Query!, cancellationToken)));
    }

    [HttpGet("tax-classifications/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ExportTaxClassifications([FromQuery] TaxClassificationReportRequest request, CancellationToken cancellationToken)
    {
        var validation = validator.Validate(request.ToQuery() with { Page = 1, PageSize = TaxClassificationReportDefaults.DefaultPageSize });
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });
        return await ExecuteAsync(async service =>
        {
            try
            {
                var file = await service.ExportTaxClassificationsAsync(validation.Query!, cancellationToken);
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        });
    }

    private async Task<IActionResult> ExecuteAsync(Func<ITaxReportQueryService, Task<IActionResult>> action)
    {
        var service = services.GetService<ITaxReportQueryService>();
        if (service is null) return DatabaseUnavailable();
        try { return await action(service); }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested) { return DatabaseUnavailable(); }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
