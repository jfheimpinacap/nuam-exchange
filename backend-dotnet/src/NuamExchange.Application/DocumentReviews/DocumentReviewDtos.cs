namespace NuamExchange.Application.DocumentReviews;

public sealed record PdfDocumentReviewResult(
    string? ReviewId,
    string FileName,
    long FileSizeBytes,
    int PageCount,
    string Status,
    string Message,
    IReadOnlyDictionary<string, string> DetectedFields,
    IReadOnlyCollection<string> MissingFields,
    IReadOnlyCollection<string> Warnings,
    string TextPreview);

public sealed record PdfDocumentReviewCommand(string FileName, long FileSizeBytes, Stream Content);

public interface IPdfDocumentReviewService
{
    Task<PdfDocumentReviewResult> ReviewAsync(PdfDocumentReviewCommand command, CancellationToken cancellationToken = default);
}

public interface IPdfTaxDocumentTextParser
{
    PdfDocumentReviewResult Parse(string fileName, long fileSizeBytes, int pageCount, string text);
}
