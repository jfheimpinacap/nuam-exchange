using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NuamExchange.XFactorTestRunner;

internal sealed class InspectionRunner(RunnerOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.Ordinal) { "Administrador", "Analista Tributario" };
    private readonly List<object> steps = [];
    private readonly List<object> errors = [];
    private readonly List<string> logLines = [];

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        DateTime startedAt = DateTime.UtcNow;
        string runId = $"{startedAt:yyyyMMdd-HHmmss}-x-factor-inspect-record-{options.RecordId}";
        string? runDirectory = null;
        JsonObject? baseline = null;
        JsonObject? authenticatedUser = null;
        string? authorizedRole = null;

        try
        {
            AddStep("validate-options", true, "Opciones locales validadas.");
            string email = ReadRequiredEnvironmentVariable("NUAM_XFACTOR_TEST_EMAIL");
            string password = ReadRequiredEnvironmentVariable("NUAM_XFACTOR_TEST_PASSWORD");
            AddStep("validate-credentials", true, "Variables de entorno requeridas presentes.");

            runDirectory = CreateRunDirectory(runId);
            Log("Carpeta externa de evidencias creada.");

            using HttpClient client = CreateHttpClient(options.ApiBaseUrl);
            LoginResponse login = await LoginAsync(client, email, password, cancellationToken);
            AddStep("login", true, "Login exitoso; token conservado solo en memoria.");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(login.TokenType, login.AccessToken);
            MeResponse me = await GetMeAsync(client, cancellationToken);
            authorizedRole = me.Role;
            authenticatedUser = new JsonObject { ["userId"] = me.Id, ["roles"] = new JsonArray(JsonValue.Create(me.Role)) };
            AddStep("authenticated-user", true, "Usuario autenticado consultado.");

            if (!AllowedRoles.Contains(me.Role))
            {
                throw new SafeFailureException("role-check", "Rol autenticado no autorizado para futuras cargas.");
            }

            AddStep("role-check", true, "Rol autorizado detectado.");
            baseline = await GetTaxClassificationAsync(client, cancellationToken);
            ValidateBaseline(baseline);
            AddStep("tax-classification", true, "Registro de prueba consultado sin cambios.");

            Console.WriteLine("API local validada");
            Console.WriteLine("Usuario autenticado: rol autorizado");
            Console.WriteLine($"Registro de prueba: {baseline["id"]}");
            Console.WriteLine($"Identidad: {baseline["market"]} / {baseline["instrumentCode"]} / {baseline["taxPeriod"]}");
            Console.WriteLine($"AppliedFactor inicial: {baseline["appliedFactor"]}");
            Console.WriteLine("Estado: inspección preparada sin cambios de datos");

            await WriteArtifactsAsync(runDirectory, runId, startedAt, DateTime.UtcNow, true, baseline, authenticatedUser, authorizedRole, cancellationToken);
            return 0;
        }
        catch (SafeFailureException ex)
        {
            AddError(ex.Operation, ex.Message);
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            AddError("network", $"Fallo de red seguro: {ex.Message}");
            Console.Error.WriteLine("Error: API local no disponible o respuesta inválida.");
        }
        catch (TaskCanceledException)
        {
            AddError("network", "La operación contra la API local excedió el tiempo permitido.");
            Console.Error.WriteLine("Error: API local no disponible o sin respuesta.");
        }

        if (runDirectory is not null)
        {
            await WriteArtifactsAsync(runDirectory, runId, startedAt, DateTime.UtcNow, false, baseline, authenticatedUser, authorizedRole, cancellationToken);
        }

        return 1;
    }

    private string ReadRequiredEnvironmentVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value)) throw new SafeFailureException("validate-credentials", $"Falta la variable de entorno requerida: {name}");
        return value;
    }

    private string CreateRunDirectory(string runId)
    {
        string path = Path.Combine(options.OutputDirectory, runId);
        Directory.CreateDirectory(path);
        return path;
    }

    private static HttpClient CreateHttpClient(Uri apiBaseUrl)
    {
        var handler = new HttpClientHandler { AllowAutoRedirect = false };
        return new HttpClient(handler) { BaseAddress = apiBaseUrl, Timeout = TimeSpan.FromSeconds(30) };
    }

    private async Task<LoginResponse> LoginAsync(HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password }, JsonOptions, cancellationToken);
        await EnsureSuccessNoRedirectAsync(response, "login", cancellationToken);
        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        if (login is null || string.IsNullOrWhiteSpace(login.AccessToken) || string.IsNullOrWhiteSpace(login.TokenType)) throw new SafeFailureException("login", "La respuesta de login no contiene token válido.");
        return login;
    }

    private async Task<MeResponse> GetMeAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync("/api/auth/me", cancellationToken);
        await EnsureSuccessNoRedirectAsync(response, "auth-me", cancellationToken);
        MeResponse? me = await response.Content.ReadFromJsonAsync<MeResponse>(JsonOptions, cancellationToken);
        if (me is null || string.IsNullOrWhiteSpace(me.Role)) throw new SafeFailureException("auth-me", "La respuesta de usuario autenticado es inesperada.");
        return me;
    }

    private async Task<JsonObject> GetTaxClassificationAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"/api/tax-classifications/{options.RecordId}", cancellationToken);
        await EnsureSuccessNoRedirectAsync(response, "tax-classification", cancellationToken);
        JsonObject? source = await response.Content.ReadFromJsonAsync<JsonObject>(JsonOptions, cancellationToken);
        if (source is null) throw new SafeFailureException("tax-classification", "La respuesta del registro es inesperada.");

        var safe = new JsonObject();
        foreach (string field in new[] { "id", "market", "instrumentCode", "instrumentName", "classificationType", "description", "updatePercentage", "appliedFactor", "referenceAmount", "currency", "taxPeriod", "validFrom", "validTo", "status", "creatorUserId", "createdAt", "updatedAt" })
        {
            if (source.TryGetPropertyValue(field, out JsonNode? value)) safe[field] = value?.DeepClone();
        }
        return safe;
    }

    private static void ValidateBaseline(JsonObject baseline)
    {
        foreach (string field in new[] { "id", "market", "instrumentCode", "taxPeriod", "appliedFactor", "updatedAt" })
        {
            if (!baseline.ContainsKey(field)) throw new SafeFailureException("tax-classification", $"La respuesta del registro no expone el campo requerido: {field}");
        }
    }

    private async Task EnsureSuccessNoRedirectAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if ((int)response.StatusCode is >= 300 and <= 399)
        {
            throw new SafeFailureException(operation, $"Respuesta de redirección rechazada. Estado HTTP {(int)response.StatusCode}.");
        }
        if (!response.IsSuccessStatusCode)
        {
            string message = await ReadSafeMessageAsync(response, cancellationToken);
            throw new SafeFailureException(operation, $"Estado HTTP {(int)response.StatusCode}: {message}");
        }
    }

    private static async Task<string> ReadSafeMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>(JsonOptions, cancellationToken);
            if (body?.TryGetPropertyValue("message", out JsonNode? message) == true) return message?.GetValue<string>() ?? "Respuesta de error sin mensaje.";
        }
        catch { }
        return response.StatusCode == HttpStatusCode.NotFound ? "Recurso no encontrado." : "Respuesta de error segura.";
    }

    private async Task WriteArtifactsAsync(string runDirectory, string runId, DateTime startedAt, DateTime finishedAt, bool success, JsonObject? baseline, JsonObject? user, string? role, CancellationToken cancellationToken)
    {
        var results = new JsonObject { ["runId"] = runId, ["startedAt"] = startedAt, ["finishedAt"] = finishedAt, ["command"] = "inspect", ["apiBaseUrl"] = options.ApiBaseUrl.ToString(), ["recordId"] = options.RecordId, ["success"] = success, ["steps"] = JsonSerializer.SerializeToNode(steps, JsonOptions), ["errors"] = JsonSerializer.SerializeToNode(errors, JsonOptions) };
        await File.WriteAllTextAsync(Path.Combine(runDirectory, "results.json"), results.ToJsonString(JsonOptions), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(runDirectory, "execution.log"), string.Join(Environment.NewLine, logLines), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(runDirectory, "baseline-tax-classification.json"), (baseline ?? new JsonObject()).ToJsonString(JsonOptions), cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(runDirectory, "authenticated-user.json"), (user ?? new JsonObject()).ToJsonString(JsonOptions), cancellationToken);
        string summary = $"# X Factor inspect\n\n- Objetivo: preparar inspección segura de un registro X Factor sin cambios de datos.\n- API local utilizada: {options.ApiBaseUrl}\n- Registro de prueba: {options.RecordId}\n- Resultado de login: {(success || user is not null ? "exitoso sin exponer secretos" : "no completado")}\n- Rol autorizado detectado: {(role is not null && AllowedRoles.Contains(role) ? role : "no autorizado/no disponible")}\n- Identidad y AppliedFactor inicial: {(baseline is not null ? $"{baseline["market"]} / {baseline["instrumentCode"]} / {baseline["taxPeriod"]}; AppliedFactor {baseline["appliedFactor"]}" : "no disponible")}\n- Ubicación de evidencias: {runDirectory}\n- Confirmación: no se ejecutaron cargas CSV ni cambios de datos.\n";
        await File.WriteAllTextAsync(Path.Combine(runDirectory, "run-summary.md"), summary, cancellationToken);
    }

    private void AddStep(string name, bool success, string message) { steps.Add(new { name, success, message, at = DateTime.UtcNow }); Log($"{name}: {message}"); }
    private void AddError(string operation, string message) { errors.Add(new { operation, message, at = DateTime.UtcNow }); Log($"ERROR {operation}: {message}"); }
    private void Log(string message) => logLines.Add($"{DateTime.UtcNow:O} {message}");

    private sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt);
    private sealed record MeResponse(int Id, string Role);
    private sealed class SafeFailureException(string operation, string message) : Exception(message) { public string Operation { get; } = operation; }
}
