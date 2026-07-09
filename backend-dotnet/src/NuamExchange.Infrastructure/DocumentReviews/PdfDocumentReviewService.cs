using System.Text;
using NuamExchange.Application.DocumentReviews;
using UglyToad.PdfPig;

namespace NuamExchange.Infrastructure.DocumentReviews;

public sealed class PdfDocumentReviewService(IPdfTaxDocumentTextParser parser) : IPdfDocumentReviewService
{
    public Task<PdfDocumentReviewResult> ReviewAsync(PdfDocumentReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            using var document = PdfDocument.Open(command.Content);
            var builder = new StringBuilder();
            var pageCount = 0;
            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                pageCount++;
                builder.AppendLine(page.Text);
            }
            return Task.FromResult(parser.Parse(command.FileName, command.FileSizeBytes, pageCount, builder.ToString()));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Task.FromResult(new PdfDocumentReviewResult(null, command.FileName, command.FileSizeBytes, 0, "INVALID_FILE", "PDF inválido: el archivo está corrupto, vacío o no corresponde a un PDF legible.", new Dictionary<string, string>(), Array.Empty<string>(), new[] { "No fue posible abrir el PDF para extracción de texto." }, string.Empty));
        }
    }
}
