using System.Globalization;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NuamExchange.XFactorTestRunner;

internal static class TaxClassificationHelpers
{
    public const int PageSize = 100;
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    public static readonly UTF8Encoding Utf8Bom = new(encoderShouldEmitUTF8Identifier: true);

    public static async Task<IReadOnlyList<JsonObject>> GetAllTaxClassificationsAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var all = new List<JsonObject>();
        int page = 1;
        int totalPages;
        do
        {
            using var response = await client.GetAsync($"/api/tax-classifications?page={page}&pageSize={PageSize}&sortBy=id&sortDirection=asc", cancellationToken);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"GET /api/tax-classifications falló con HTTP {(int)response.StatusCode}");
            JsonObject body = await response.Content.ReadFromJsonAsync<JsonObject>(JsonOptions, cancellationToken) ?? throw new InvalidOperationException("Respuesta paginada inesperada.");
            if (body["items"] is not JsonArray items) throw new InvalidOperationException("La respuesta paginada no contiene items.");
            foreach (JsonNode? item in items)
            {
                if (item is JsonObject obj) all.Add(obj);
            }
            int pageSize = GetInt(body, "pageSize") ?? PageSize;
            int totalCount = GetInt(body, "totalCount") ?? all.Count;
            totalPages = GetInt(body, "totalPages") ?? (totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize));
            page++;
        }
        while (page <= totalPages);
        return all;
    }

    public static IReadOnlyList<JsonObject> FindIdentityMatches(IEnumerable<JsonObject> records, JsonObject baseline)
    {
        string market = StringValue(baseline, "market") ?? string.Empty;
        string instrumentCode = StringValue(baseline, "instrumentCode") ?? string.Empty;
        int? taxPeriod = GetInt(baseline, "taxPeriod");
        return records.Where(r => string.Equals(StringValue(r, "market"), market, StringComparison.Ordinal)
            && string.Equals(StringValue(r, "instrumentCode"), instrumentCode, StringComparison.Ordinal)
            && GetInt(r, "taxPeriod") == taxPeriod).Select(SafeProjection).ToList();
    }

    public static JsonObject SafeProjection(JsonObject source)
    {
        var safe = new JsonObject();
        foreach (string field in new[] { "id", "market", "instrumentCode", "taxPeriod", "appliedFactor", "status" })
        {
            if (source.TryGetPropertyValue(field, out JsonNode? value)) safe[field] = value?.DeepClone();
        }
        return safe;
    }

    public static bool IsEligible(JsonObject record) =>
        GetInt(record, "id") is > 0 &&
        !string.IsNullOrWhiteSpace(StringValue(record, "market")) &&
        !string.IsNullOrWhiteSpace(StringValue(record, "instrumentCode")) &&
        GetInt(record, "taxPeriod") is not null &&
        GetDecimal(record, "appliedFactor") is not null;

    public static string IdentityKey(JsonObject record) => string.Join("\u001f", StringValue(record, "market") ?? string.Empty, StringValue(record, "instrumentCode") ?? string.Empty, GetInt(record, "taxPeriod")?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
    public static int? GetInt(JsonNode? n, string p) => n?[p]?.GetValue<int>();
    public static string? StringValue(JsonNode? n, string p) => n?[p]?.GetValue<string>();
    public static decimal? GetDecimal(JsonNode? n, string p) => decimal.TryParse(n?[p]?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null;
    public static string CsvQuote(string value) => '"' + value.Replace("\"", "\"\"") + '"';
}
