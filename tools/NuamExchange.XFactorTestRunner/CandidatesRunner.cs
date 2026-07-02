using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NuamExchange.XFactorTestRunner;

internal sealed record CandidatesOptions(Uri ApiBaseUrl, int Limit, string OutputDirectory);

internal sealed class CandidatesRunner(CandidatesOptions options)
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.Ordinal) { "Administrador", "Analista Tributario" };
    private readonly List<string> log = [];

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        DateTime started = DateTime.UtcNow;
        string runId = $"{started:yyyyMMdd-HHmmss}-x-factor-candidates";
        string runDirectory = Path.Combine(options.OutputDirectory, runId);
        Directory.CreateDirectory(runDirectory);
        try
        {
            string email = Env("NUAM_XFACTOR_TEST_EMAIL");
            string password = Env("NUAM_XFACTOR_TEST_PASSWORD");
            using HttpClient client = CreateHttpClient(options.ApiBaseUrl);
            LoginResponse login = await LoginAsync(client, email, password, cancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(login.TokenType, login.AccessToken);
            MeResponse me = await GetMeAsync(client, cancellationToken);
            if (!AllowedRoles.Contains(me.Role)) throw new InvalidOperationException("Rol autenticado no autorizado para consultar candidatos X Factor.");
            Log("Login y rol validados sin exponer secretos.");

            IReadOnlyList<JsonObject> records = await TaxClassificationHelpers.GetAllTaxClassificationsAsync(client, cancellationToken);
            var counts = records.GroupBy(TaxClassificationHelpers.IdentityKey).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
            var eligible = records.Where(TaxClassificationHelpers.IsEligible).Where(r => counts[TaxClassificationHelpers.IdentityKey(r)] == 1).Select(TaxClassificationHelpers.SafeProjection).Take(options.Limit).ToList();
            Log($"Registros consultados: {records.Count}; candidatos exportados: {eligible.Count}.");

            await WriteCsvAsync(Path.Combine(runDirectory, "eligible-x-factor-candidates.csv"), eligible, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(runDirectory, "results.json"), new JsonObject { ["command"] = "candidates", ["startedAt"] = started, ["finishedAt"] = DateTime.UtcNow, ["apiBaseUrl"] = options.ApiBaseUrl.ToString(), ["limit"] = options.Limit, ["recordsScanned"] = records.Count, ["eligibleExported"] = eligible.Count }.ToJsonString(TaxClassificationHelpers.JsonOptions), cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(runDirectory, "execution.log"), string.Join(Environment.NewLine, log), TaxClassificationHelpers.Utf8Bom, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(runDirectory, "candidate-summary.md"), BuildSummary(runDirectory, records.Count, eligible.Count), TaxClassificationHelpers.Utf8Bom, cancellationToken);

            foreach (JsonObject c in eligible)
            {
                Console.WriteLine($"{c["id"]} | {c["market"]} | {c["instrumentCode"]} | {c["taxPeriod"]} | {c["appliedFactor"]} | {c["status"]}");
            }
            Console.WriteLine("Seleccione manualmente un ID elegible y úselo luego con inspect o run.");
            Console.WriteLine($"Evidencias: {runDirectory}");
            return 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException or JsonException)
        {
            Log("ERROR: " + Safe(ex.Message));
            await File.WriteAllTextAsync(Path.Combine(runDirectory, "execution.log"), string.Join(Environment.NewLine, log), TaxClassificationHelpers.Utf8Bom, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(runDirectory, "results.json"), new JsonObject { ["command"] = "candidates", ["startedAt"] = started, ["finishedAt"] = DateTime.UtcNow, ["success"] = false, ["error"] = Safe(ex.Message) }.ToJsonString(TaxClassificationHelpers.JsonOptions), cancellationToken);
            Console.Error.WriteLine("Error: no fue posible calcular candidatos. Revise evidencias externas.");
            return 1;
        }
    }

    private static async Task WriteCsvAsync(string path, IEnumerable<JsonObject> rows, CancellationToken ct)
    {
        var sb = new StringBuilder("id,market,instrumentCode,taxPeriod,appliedFactor,status\r\n");
        foreach (JsonObject r in rows)
        {
            sb.AppendJoin(',', new[] { "id", "market", "instrumentCode", "taxPeriod", "appliedFactor", "status" }.Select(f => TaxClassificationHelpers.CsvQuote(r[f]?.ToString() ?? string.Empty))).Append("\r\n");
        }
        await File.WriteAllTextAsync(path, sb.ToString(), ct);
    }

    private string BuildSummary(string dir, int scanned, int exported) => $"# X Factor candidates\n\n- Propósito: encontrar registros locales elegibles sin modificar datos.\n- API local utilizada: {options.ApiBaseUrl}\n- Registros consultados: {scanned}\n- Candidatos exportados: {exported}\n- Criterio: id, market, instrumentCode, taxPeriod y appliedFactor válidos, con identidad market + instrumentCode + taxPeriod única.\n- Evidencias: {dir}\n- Confirmación: no se llamaron endpoints de carga ni se modificaron datos.\n";
    private static HttpClient CreateHttpClient(Uri apiBaseUrl) { var handler = new HttpClientHandler { AllowAutoRedirect = false }; return new HttpClient(handler) { BaseAddress = apiBaseUrl, Timeout = TimeSpan.FromSeconds(30) }; }
    private static string Env(string name) => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)) ? throw new InvalidOperationException($"Falta la variable de entorno requerida: {name}") : Environment.GetEnvironmentVariable(name)!;
    private async Task<LoginResponse> LoginAsync(HttpClient client, string email, string password, CancellationToken ct) { using var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password }, TaxClassificationHelpers.JsonOptions, ct); if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"Login falló con HTTP {(int)response.StatusCode}"); var login = await response.Content.ReadFromJsonAsync<LoginResponse>(TaxClassificationHelpers.JsonOptions, ct); if (login is null || string.IsNullOrWhiteSpace(login.AccessToken)) throw new InvalidOperationException("Login sin token válido."); return login; }
    private async Task<MeResponse> GetMeAsync(HttpClient client, CancellationToken ct) { using var response = await client.GetAsync("/api/auth/me", ct); if (!response.IsSuccessStatusCode) throw new InvalidOperationException($"/api/auth/me falló con HTTP {(int)response.StatusCode}"); return await response.Content.ReadFromJsonAsync<MeResponse>(TaxClassificationHelpers.JsonOptions, ct) ?? throw new InvalidOperationException("Usuario autenticado inesperado."); }
    private void Log(string m) => log.Add($"{DateTime.UtcNow:O} {Safe(m)}");
    private static string Safe(string s) => s.Replace(Environment.GetEnvironmentVariable("NUAM_XFACTOR_TEST_EMAIL") ?? "\0", "[redacted]").Replace(Environment.GetEnvironmentVariable("NUAM_XFACTOR_TEST_PASSWORD") ?? "\0", "[redacted]");
    private sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt);
    private sealed record MeResponse(int Id, string Role);
}
