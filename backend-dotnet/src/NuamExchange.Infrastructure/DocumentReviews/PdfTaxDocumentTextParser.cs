using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NuamExchange.Application.DocumentReviews;

namespace NuamExchange.Infrastructure.DocumentReviews;

public sealed class PdfTaxDocumentTextParser : IPdfTaxDocumentTextParser
{
    private static readonly (string Key, string Label, Regex Pattern)[] ExpectedFields =
    [
        ("documentType", "Tipo de documento", BuildFieldRegex("Tipo\\s+de\\s+documento")),
        ("market", "Mercado", BuildFieldRegex("Mercado")),
        ("instrument", "Instrumento", BuildFieldRegex("Instrumento")),
        ("taxPeriod", "Periodo tributario", BuildFieldRegex("Per[ií]odo\\s+tributario")),
        ("appliedFactor", "Factor aplicado", BuildFieldRegex("Factor\\s+aplicado")),
        ("referenceAmount", "Monto de referencia", BuildFieldRegex("Monto\\s+de\\s+referencia")),
        ("issueDate", "Fecha de emisión", BuildFieldRegex("Fecha\\s+de\\s+emisi[oó]n")),
    ];

    public PdfDocumentReviewResult Parse(string fileName, long fileSizeBytes, int pageCount, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result(fileName, fileSizeBytes, pageCount, "UNSUPPORTED", "PDF no compatible: el archivo no contiene texto seleccionable. OCR queda fuera del alcance de esta versión.", new Dictionary<string, string>(), ExpectedFields.Select(x => x.Label).ToArray(), ["No se extrajo texto seleccionable desde el PDF."], string.Empty);
        }

        var detected = new Dictionary<string, string>();
        foreach (var field in ExpectedFields)
        {
            var match = field.Pattern.Match(text);
            if (match.Success)
            {
                detected[field.Key] = CleanValue(match.Groups["value"].Value);
            }
        }

        var missing = ExpectedFields.Where(x => !detected.ContainsKey(x.Key)).Select(x => x.Label).ToArray();
        var preview = BuildPreview(text);

        if (missing.Length == 0)
        {
            return Result(fileName, fileSizeBytes, pageCount, "VALID", "PDF válido: contiene todos los campos tributarios requeridos.", detected, missing, Array.Empty<string>(), preview);
        }

        if (detected.Count >= 2)
        {
            return Result(fileName, fileSizeBytes, pageCount, "INCOMPLETE", "PDF incompleto: faltan campos obligatorios.", detected, missing, Array.Empty<string>(), preview);
        }

        return Result(fileName, fileSizeBytes, pageCount, "UNSUPPORTED", "PDF no compatible: no se detectó estructura tributaria reconocida.", detected, missing, ["El documento no coincide con la estructura tributaria esperada para esta versión Lite."], preview);
    }

    private static Regex BuildFieldRegex(string labelPattern) => new($@"(?im)^\s*{labelPattern}\s*:\s*(?<value>.+?)\s*$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static string CleanValue(string value) => Regex.Replace(value.Trim(), "\\s+", " ");
    private static string BuildPreview(string text)
    {
        var normalized = Regex.Replace(text.Trim(), "[ \t]+", " ");
        return normalized.Length <= 1200 ? normalized : string.Concat(normalized.AsSpan(0, 1200), "...");
    }
    private static PdfDocumentReviewResult Result(string fileName, long fileSizeBytes, int pageCount, string status, string message, IReadOnlyDictionary<string, string> detected, IReadOnlyCollection<string> missing, IReadOnlyCollection<string> warnings, string preview)
        => new(null, fileName, fileSizeBytes, pageCount, status, message, detected, missing, warnings, preview);
}
