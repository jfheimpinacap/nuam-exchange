using System.Text.RegularExpressions;
using NuamExchange.Application.DocumentReviews;

namespace NuamExchange.Infrastructure.DocumentReviews;

public sealed class PdfTaxDocumentTextParser : IPdfTaxDocumentTextParser
{
    private static readonly FieldDefinition[] ExpectedFields =
    [
        new("documentType", "Tipo de documento", @"Tipo\s+de\s+documento"),
        new("market", "Mercado", @"Mercado"),
        new("instrument", "Instrumento", @"Instrumento"),
        new("taxPeriod", "Periodo tributario", @"Per(?:i|í)odo\s+tributario"),
        new("appliedFactor", "Factor aplicado", @"Factor\s+aplicado"),
        new("referenceAmount", "Monto de referencia", @"Monto\s+de\s+referencia"),
        new("issueDate", "Fecha de emisión", @"Fecha\s+de\s+emisi(?:o|ó|\?\?)n"),
    ];

    private static readonly Regex FieldLabelRegex = new(
        $@"(?<label>{string.Join('|', ExpectedFields.Select(field => field.LabelPattern))})\s*:",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public PdfDocumentReviewResult Parse(string fileName, long fileSizeBytes, int pageCount, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result(fileName, fileSizeBytes, pageCount, "UNSUPPORTED", "PDF no compatible: el archivo no contiene texto seleccionable. OCR queda fuera del alcance de esta versión.", new Dictionary<string, string>(), ExpectedFields.Select(x => x.Label).ToArray(), ["No se extrajo texto seleccionable desde el PDF."], string.Empty);
        }

        var detected = ExtractDetectedFields(text);
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

    private static Dictionary<string, string> ExtractDetectedFields(string text)
    {
        var labels = FieldLabelRegex.Matches(text)
            .Select(match => new FoundLabel(GetFieldDefinition(match), match.Index, match.Index + match.Length))
            .OrderBy(label => label.Start)
            .ToArray();

        var detected = new Dictionary<string, string>();
        for (var i = 0; i < labels.Length; i++)
        {
            var current = labels[i];
            var valueEnd = i + 1 < labels.Length ? labels[i + 1].Start : text.Length;
            var value = CleanValue(text[current.ValueStart..valueEnd]);
            if (!string.IsNullOrWhiteSpace(value))
            {
                detected[current.Field.Key] = value;
            }
        }

        return detected;
    }

    private static FieldDefinition GetFieldDefinition(Match match)
    {
        var label = match.Groups["label"].Value;
        return ExpectedFields.First(field => Regex.IsMatch(label, $@"^{field.LabelPattern}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
    }

    private static string CleanValue(string value) => Regex.Replace(value.Trim(), "\\s+", " ");
    private static string BuildPreview(string text)
    {
        var normalized = Regex.Replace(text.Trim(), "[ \t]+", " ");
        return normalized.Length <= 1200 ? normalized : string.Concat(normalized.AsSpan(0, 1200), "...");
    }
    private static PdfDocumentReviewResult Result(string fileName, long fileSizeBytes, int pageCount, string status, string message, IReadOnlyDictionary<string, string> detected, IReadOnlyCollection<string> missing, IReadOnlyCollection<string> warnings, string preview)
        => new(null, fileName, fileSizeBytes, pageCount, status, message, detected, missing, warnings, preview);

    private sealed record FieldDefinition(string Key, string Label, string LabelPattern);
    private sealed record FoundLabel(FieldDefinition Field, int Start, int ValueStart);
}
