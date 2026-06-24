using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Api.Contracts.TaxClassifications;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/tax-classifications")]
[Authorize(Policy = "TaxClassificationRead")]
public sealed class TaxClassificationsController(IServiceProvider services, ITaxClassificationQueryValidator queryValidator, ICreateTaxClassificationValidator createValidator, IUpdateTaxClassificationValidator updateValidator, ISupervisorValidationTaxClassificationValidator supervisorValidationValidator) : ControllerBase
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


    [HttpPost]
    [Authorize(Policy = "TaxClassificationWrite")]
    [ProducesResponseType(typeof(TaxClassificationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create([FromBody] CreateTaxClassificationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "No fue posible identificar al usuario autenticado." });

        var validation = createValidator.Validate(request.ToCommand(userId, HttpContext.Connection.RemoteIpAddress?.ToString()));
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });

        return await ExecuteCommandAsync(async service =>
        {
            var created = await service.CreateAsync(validation.Command!, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "TaxClassificationWrite")]
    [ProducesResponseType(typeof(TaxClassificationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxClassificationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "No fue posible identificar al usuario autenticado." });

        var validation = updateValidator.Validate(request.ToCommand(id, userId, HttpContext.Connection.RemoteIpAddress?.ToString()));
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });

        return await ExecuteCommandAsync(async service =>
        {
            var updated = await service.UpdateAsync(validation.Command!, cancellationToken);
            return updated is null ? NotFound(new { message = "La calificación tributaria no existe." }) : Ok(updated);
        });
    }



    [HttpPost("{id:int}/supervisor-validation")]
    [Authorize(Policy = "TaxClassificationSupervise")]
    [ProducesResponseType(typeof(TaxClassificationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SupervisorValidation(int id, [FromBody] SupervisorValidationTaxClassificationRequest? request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "No fue posible identificar al usuario autenticado." });
        if (request is null) return BadRequest(new { message = "El body de validación supervisora es obligatorio." });

        var validation = supervisorValidationValidator.Validate(request.ToCommand(id, userId, HttpContext.Connection.RemoteIpAddress?.ToString()));
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });

        return await ExecuteCommandAsync(async service =>
        {
            var result = await service.SupervisorValidationAsync(validation.Command!, cancellationToken);
            return result.Succeeded ? Ok(result.Value) : StatusCode(result.StatusCode, new { message = result.Message });
        });
    }

    [HttpPost("{id:int}/copy")]
    [Authorize(Policy = "TaxClassificationWrite")]
    [ProducesResponseType(typeof(TaxClassificationDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Copy(int id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "No fue posible identificar al usuario autenticado." });

        return await ExecuteCommandAsync(async service =>
        {
            var copied = await service.CopyAsync(new CopyTaxClassificationCommand(id, userId, HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken);
            return copied is null ? NotFound(new { message = "La calificación tributaria no existe." }) : CreatedAtAction(nameof(GetById), new { id = copied.Id }, copied);
        });
    }


    [HttpPost("bulk-loads/x-factor")]
    [Authorize(Policy = "TaxClassificationWrite")]
    [ProducesResponseType(typeof(BulkLoadXFactorResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> BulkLoadXFactor([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized(new { message = "No fue posible identificar al usuario autenticado." });
        var validation = await ValidateCsvFileAsync(file, cancellationToken);
        if (!validation.Succeeded) return BadRequest(new { message = validation.Message });

        return await ExecuteCommandAsync(async service => Ok(await service.BulkLoadXFactorAsync(new BulkLoadXFactorCommand(userId, file!.FileName, file.Length, validation.Content!, HttpContext.Connection.RemoteIpAddress?.ToString()), cancellationToken)));
    }

    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TaxClassificationHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHistory(int id, CancellationToken cancellationToken)
        => await ExecuteAsync(async service =>
        {
            var history = await service.GetHistoryAsync(id, cancellationToken);
            return history is null ? NotFound(new { message = "La calificación tributaria no existe." }) : Ok(history);
        });

    [HttpGet("filter-options")]
    [ProducesResponseType(typeof(TaxClassificationFilterOptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetFilterOptions(CancellationToken cancellationToken)
        => await ExecuteAsync(async service => Ok(await service.GetFilterOptionsAsync(cancellationToken)));


    private static async Task<CsvFileValidationResult> ValidateCsvFileAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return CsvFileValidationResult.Failure("El campo multipart file es obligatorio.");
        if (file.Length <= 0) return CsvFileValidationResult.Failure("El archivo CSV no puede estar vacío.");
        if (!string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.OrdinalIgnoreCase)) return CsvFileValidationResult.Failure("Solo se aceptan archivos CSV.");

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        string content;
        try { content = await reader.ReadToEndAsync(cancellationToken); }
        catch (DecoderFallbackException) { return CsvFileValidationResult.Failure("El archivo debe estar codificado en UTF-8 válido."); }

        if (string.IsNullOrWhiteSpace(content)) return CsvFileValidationResult.Failure("El archivo CSV no puede estar vacío.");
        var normalized = content.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var header = lines.FirstOrDefault()?.Trim('\uFEFF').Trim();
        if (!string.Equals(header, "market;instrumentCode;taxPeriod;appliedFactor", StringComparison.OrdinalIgnoreCase)) return CsvFileValidationResult.Failure("El encabezado CSV debe ser market;instrumentCode;taxPeriod;appliedFactor.");
        if (!lines.Skip(1).Any(line => !string.IsNullOrWhiteSpace(line))) return CsvFileValidationResult.Failure("El archivo CSV debe contener al menos una fila de datos.");
        return CsvFileValidationResult.Success(normalized);
    }

    private sealed record CsvFileValidationResult(bool Succeeded, string? Content, string? Message)
    {
        public static CsvFileValidationResult Success(string content) => new(true, content, null);
        public static CsvFileValidationResult Failure(string message) => new(false, null, message);
    }

    private async Task<IActionResult> ExecuteCommandAsync(Func<ITaxClassificationCommandService, Task<IActionResult>> action)
    {
        var service = services.GetService<ITaxClassificationCommandService>();
        if (service is null) return DatabaseUnavailable();

        try { return await action(service); }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested) { return DatabaseUnavailable(); }
    }

    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value, out userId);

    private async Task<IActionResult> ExecuteAsync(Func<ITaxClassificationQueryService, Task<IActionResult>> action)
    {
        var service = services.GetService<ITaxClassificationQueryService>();
        if (service is null) return DatabaseUnavailable();

        try { return await action(service); }
        catch (Exception) when (!HttpContext.RequestAborted.IsCancellationRequested) { return DatabaseUnavailable(); }
    }

    private ObjectResult DatabaseUnavailable() => StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos no está disponible para procesar la solicitud." });
}
