using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuamExchange.Application.DocumentReviews;

namespace NuamExchange.Api.Controllers;

[ApiController]
[Route("api/document-reviews")]
public sealed class DocumentReviewsController(IPdfDocumentReviewService service) : ControllerBase
{
    private const long MaxPdfBytes = 10 * 1024 * 1024;

    [HttpPost("pdf")]
    [Authorize(Policy = "DocumentReviewWrite")]
    [RequestSizeLimit(MaxPdfBytes)]
    [ProducesResponseType(typeof(PdfDocumentReviewResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReviewPdf([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null) return BadRequest(new ProblemDetails { Title = "Debe adjuntar un archivo PDF en el campo file.", Status = StatusCodes.Status400BadRequest });
        if (file.Length == 0) return BadRequest(new ProblemDetails { Title = "El archivo PDF está vacío.", Status = StatusCodes.Status400BadRequest });
        if (file.Length > MaxPdfBytes) return BadRequest(new ProblemDetails { Title = "El archivo PDF supera el tamaño máximo permitido de 10 MB.", Status = StatusCodes.Status400BadRequest });
        if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) return BadRequest(new ProblemDetails { Title = "El archivo debe tener extensión .pdf.", Status = StatusCodes.Status400BadRequest });

        await using var stream = file.OpenReadStream();
        var result = await service.ReviewAsync(new PdfDocumentReviewCommand(Path.GetFileName(file.FileName), file.Length, stream), cancellationToken);
        return Ok(result);
    }
}
