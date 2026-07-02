using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NuamExchange.XFactorTestRunner;

internal sealed class RunRunner(RunOptions options)
{
    private const string Header = "market;instrumentCode;taxPeriod;appliedFactor";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.Ordinal) { "Administrador", "Analista Tributario" };
    private readonly List<MatrixRow> matrix = [];
    private readonly List<string> log = [];
    private string? runDirectory;
    private string? csvDirectory;
    private string? responsesDirectory;
    private JsonObject? baseline;
    private bool mayNeedRestore;
    private bool restorationOk;
    private bool noModificationRestoreNotRequired;
    private string baselineFactor = string.Empty;

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        if (!options.ConfirmWrite)
        {
            Console.Error.WriteLine("Error: run requiere la bandera literal --confirm-write antes de cualquier login, CSV o llamada HTTP.");
            return 1;
        }

        DateTime started = DateTime.UtcNow;
        string runId = $"{started:yyyyMMdd-HHmmss}-x-factor-run-record-{options.RecordId}";
        JsonObject? user = null;
        JsonNode? historyBefore = null;
        JsonNode? historyAfter = null;
        JsonObject restoration = new() { ["attempted"] = false, ["success"] = false };

        try
        {
            runDirectory = Path.Combine(options.OutputDirectory, runId);
            csvDirectory = Path.Combine(runDirectory, "csv");
            responsesDirectory = Path.Combine(runDirectory, "responses");
            Directory.CreateDirectory(csvDirectory);
            Directory.CreateDirectory(responsesDirectory);
            Log("Carpeta externa de evidencias creada.");

            string email = Env("NUAM_XFACTOR_TEST_EMAIL");
            string password = Env("NUAM_XFACTOR_TEST_PASSWORD");
            using HttpClient client = CreateHttpClient(options.ApiBaseUrl);
            LoginResponse login = await LoginAsync(client, email, password, cancellationToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(login.TokenType, login.AccessToken);
            MeResponse me = await GetMeAsync(client, cancellationToken);
            user = new JsonObject { ["userId"] = me.Id, ["roles"] = new JsonArray(JsonValue.Create(me.Role)) };
            if (!AllowedRoles.Contains(me.Role)) throw new SafeFailureException("Rol autenticado no autorizado para cargas X Factor.");

            baseline = await GetTaxClassificationAsync(client, cancellationToken);
            await WriteJsonAsync("baseline-tax-classification.json", baseline, cancellationToken);
            ValidateIdentityAndFactor(baseline);
            baselineFactor = DecimalText(GetDecimal(baseline, "appliedFactor")!.Value);
            IReadOnlyList<JsonObject> records = await TaxClassificationHelpers.GetAllTaxClassificationsAsync(client, cancellationToken);
            IReadOnlyList<JsonObject> identityMatches = TaxClassificationHelpers.FindIdentityMatches(records, baseline);
            await WriteJsonAsync("run-precondition-identity-matches.json", JsonSerializer.SerializeToNode(identityMatches, JsonOptions) ?? new JsonArray(), cancellationToken);
            bool uniqueIdentity = identityMatches.Count == 1 && identityMatches[0]["id"]?.GetValue<int>() == options.RecordId;
            if (!uniqueIdentity)
            {
                AddMatrix("RUN-PRECONDITION", "Identidad única requerida", "Una coincidencia exacta para recordId", $"Coincidencias={identityMatches.Count}", "FAIL", null, null, "No hubo modificación porque la identidad del registro no era única.");
                AddNotExecutedCases("No ejecutado por precondición fallida de identidad única.");
                noModificationRestoreNotRequired = true;
                throw new SafeFailureException("Identidad no única: no se ejecutan cargas X Factor.");
            }
            historyBefore = await GetJsonAsync(client, $"/api/tax-classifications/{options.RecordId}/history", cancellationToken);
            await WriteJsonAsync("history-before.json", historyBefore, cancellationToken);

            var factors = PickFactors(GetDecimal(baseline, "appliedFactor")!.Value);
            await WriteCsvFilesAsync(factors, runId, cancellationToken);

            var f01Upload = await CaseUpload(client, "XF-01", "archivo válido", "XF-01-valido.csv", HttpStatusCode.OK, cancellationToken, expectedTotal:1, expectedOk:1, expectedFail:0, expectUpdated:true);
            string f01 = f01Upload.UploadId;
            await ExpectFactorAsync(client, "XF-01", factors[0], cancellationToken);
            var afterXf01 = await GetTaxClassificationAsync(client, cancellationToken);
            mayNeedRestore = f01Upload.Status == HttpStatusCode.OK && Int(f01Upload.Body, "successfulRows") > 0 && ContainsUpdatedId(f01Upload.Body, options.RecordId) && !SameFactor(afterXf01, GetDecimal(baseline, "appliedFactor")!.Value);
            await TraceUploadAsync(client, "XF-01", f01, 1, 0, cancellationToken);

            await CaseNoFile(client, cancellationToken); await ExpectFactorAsync(client, "XF-02", factors[0], cancellationToken);
            await CaseUpload(client, "XF-03", "extensión incorrecta", "XF-03-extension-incorrecta.txt", HttpStatusCode.BadRequest, cancellationToken); await ExpectFactorAsync(client, "XF-03", factors[0], cancellationToken);
            await CaseUpload(client, "XF-04", "encabezado incorrecto", "XF-04-encabezado-incorrecto.csv", HttpStatusCode.BadRequest, cancellationToken); await ExpectFactorAsync(client, "XF-04", factors[0], cancellationToken);
            await CaseUpload(client, "XF-05", "factor inválido", "XF-05-factor-invalido.csv", HttpStatusCode.OK, cancellationToken, expectedTotal:1, expectedOk:0, expectedFail:1, expectedCodes:["INVALID_APPLIED_FACTOR"]); await ExpectFactorAsync(client, "XF-05", factors[0], cancellationToken);
            await CaseUpload(client, "XF-06", "registro inexistente", "XF-06-no-encontrado.csv", HttpStatusCode.OK, cancellationToken, expectedTotal:1, expectedOk:0, expectedFail:1, expectedCodes:["NOT_FOUND"]); await ExpectFactorAsync(client, "XF-06", factors[0], cancellationToken);
            await CaseUpload(client, "XF-07", "identidad duplicada", "XF-07-duplicado.csv", HttpStatusCode.OK, cancellationToken, expectedTotal:2, expectedOk:1, expectedFail:1, expectedCodes:["DUPLICATE_ROW"]); await ExpectFactorAsync(client, "XF-07", factors[1], cancellationToken);
            var f08Upload = await CaseUpload(client, "XF-08", "archivo mixto", "XF-08-mixto.csv", HttpStatusCode.OK, cancellationToken, expectedTotal:3, expectedOk:1, expectedFail:2, expectedCodes:["NOT_FOUND","INVALID_APPLIED_FACTOR"]); await ExpectFactorAsync(client, "XF-08", factors[2], cancellationToken);
            await TraceUploadAsync(client, "XF-08", f08Upload.UploadId, 3, 2, cancellationToken);
            await CaseNoToken(cancellationToken); await ExpectFactorAsync(client, "XF-09", factors[2], cancellationToken);
            AddMatrix("XF-10", "Supervisor sin permiso", "NO EJECUTADO MANUALMENTE", "Cubierto por pruebas xUnit existentes; no se creó una cuenta local solo para probar permisos.", "NOT_EXECUTED", null, null, "Pendiente de cuenta Supervisor autorizada.");
        }
        catch (Exception ex) when (ex is SafeFailureException or HttpRequestException or TaskCanceledException or JsonException or IOException)
        {
            Log("ERROR: " + Safe(ex.Message));
            if (!matrix.Any(r => r.Status == "FAIL")) AddMatrix("RUN", "Ejecución", "Completar flujo seguro", Safe(ex.Message), "FAIL", null, null, "Fallo controlado.");
            Console.Error.WriteLine("Error: la ejecución falló. Revise evidencias externas si fueron creadas.");
        }
        finally
        {
            if (runDirectory is not null)
            {
                if (mayNeedRestore && baseline is not null)
                {
                    restoration = await RestoreAsync(restoration, cancellationToken);
                }
                else if (baseline is not null && !matrix.Any(r => r.CaseId == "RESTORE"))
                {
                    JsonObject? current = await TryWithAuthTaxClassificationAsync(cancellationToken);
                    if (current is not null) await WriteJsonAsync("post-run-tax-classification.json", current, cancellationToken);
                    bool unchanged = current is not null && SameFactor(current, GetDecimal(baseline, "appliedFactor")!.Value) && SameBusinessFields(baseline, current);
                    noModificationRestoreNotRequired = unchanged;
                    restoration["attempted"] = false;
                    restoration["success"] = unchanged;
                    restoration["reason"] = "No hubo modificación exitosa; restauración no requerida.";
                    AddMatrix("RESTORE", "Restauración factor original", "No requerida sin modificación exitosa", unchanged ? "Factor final coincide con baseline" : "Estado final no confirmado", unchanged ? "NOT_EXECUTED" : "FAIL", null, null, unchanged ? "No hubo modificación exitosa." : "No se pudo confirmar estado final sin cambios.");
                }
                historyAfter = await TryWithAuthJsonAsync($"/api/tax-classifications/{options.RecordId}/history", cancellationToken);
                await WriteJsonAsync("history-after.json", historyAfter ?? new JsonArray(), cancellationToken);
                await WriteJsonAsync("authenticated-user.json", user ?? new JsonObject(), cancellationToken);
                await WriteJsonAsync("restoration-result.json", restoration, cancellationToken);
                await WriteJsonAsync("results.json", BuildResults(started, DateTime.UtcNow, restoration), cancellationToken);
                await WriteMatrixAsync(cancellationToken);
                await WriteSummaryAsync(cancellationToken);
                await File.WriteAllTextAsync(Path.Combine(runDirectory, "execution.log"), string.Join(Environment.NewLine, log), TaxClassificationHelpers.Utf8Bom, cancellationToken);
            }
        }

        bool ok = matrix.All(x => x.Status != "FAIL") && (!mayNeedRestore || restorationOk);
        Console.WriteLine($"Evidencias: {runDirectory}");
        Console.WriteLine(restorationOk ? "Restauración exitosa." : mayNeedRestore ? $"ADVERTENCIA: restauración fallida o no confirmada. Baseline pendiente: {baselineFactor}. Evidencias: {runDirectory}" : "Restauración no requerida.");
        return ok ? 0 : 1;
    }

    private async Task<JsonObject> RestoreAsync(JsonObject restoration, CancellationToken ct)
    {
        restoration["attempted"] = true;
        try
        {
            using HttpClient client = CreateHttpClient(options.ApiBaseUrl);
            string email = Env("NUAM_XFACTOR_TEST_EMAIL"); string password = Env("NUAM_XFACTOR_TEST_PASSWORD");
            LoginResponse login = await LoginAsync(client, email, password, ct);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(login.TokenType, login.AccessToken);
            var result = await UploadAsync(client, "RESTORE-factor-original.csv", true, ct);
            await SaveResponseAsync("RESTORE", result.Body, ct);
            var current = await GetTaxClassificationAsync(client, ct);
            await WriteJsonAsync("post-run-tax-classification.json", current, ct);
            bool ok = result.Status == HttpStatusCode.OK && Int(result.Body,"successfulRows") == 1 && SameFactor(current, GetDecimal(baseline!, "appliedFactor")!.Value) && SameBusinessFields(baseline!, current);
            restorationOk = ok;
            restoration["success"] = ok; restoration["httpStatus"] = (int)result.Status; restoration["baselineAppliedFactor"] = baselineFactor;
            AddMatrix("RESTORE", "Restauración factor original", "AppliedFactor baseline restaurado", ok ? "Restaurado" : "No restaurado", ok ? "PASS" : "FAIL", (int)result.Status, Str(result.Body,"uploadId"), null);
        }
        catch (Exception ex) when (ex is SafeFailureException or HttpRequestException or TaskCanceledException or IOException or JsonException)
        { restoration["success"] = false; restoration["error"] = Safe(ex.Message); AddMatrix("RESTORE", "Restauración factor original", "AppliedFactor baseline restaurado", Safe(ex.Message), "FAIL", null, null, "Fallo crítico de restauración."); }
        return restoration;
    }

    private async Task<UploadResult> CaseUpload(HttpClient client, string id, string name, string file, HttpStatusCode status, CancellationToken ct, int? expectedTotal=null, int? expectedOk=null, int? expectedFail=null, string[]? expectedCodes=null, bool expectUpdated=false)
    {
        var r = await UploadAsync(client, file, true, ct);
        bool ok = r.Status == status;
        if (expectedTotal.HasValue) ok &= Int(r.Body,"totalRows") == expectedTotal && Int(r.Body,"successfulRows") == expectedOk && Int(r.Body,"failedRows") == expectedFail;
        if (expectedCodes is not null) ok &= expectedCodes.All(c => ContainsText(r.Body, c));
        if (expectUpdated) ok &= ContainsText(r.Body, options.RecordId.ToString(CultureInfo.InvariantCulture));
        if (id is "XF-01" or "XF-05" or "XF-06" or "XF-07" or "XF-08") await SaveResponseAsync(id, r.Body, ct);
        AddMatrix(id, name, status.ToString(), Summ(r.Body), ok ? "PASS" : "FAIL", (int)r.Status, Str(r.Body,"uploadId"), null);
        return new UploadResult(r.Status, r.Body, Str(r.Body,"uploadId") ?? string.Empty);
    }
    private async Task CaseNoFile(HttpClient client, CancellationToken ct) { using var form = new MultipartFormDataContent(); using var resp = await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", form, ct); AddMatrix("XF-02","falta el campo file","HTTP 400",((int)resp.StatusCode).ToString(),resp.StatusCode==HttpStatusCode.BadRequest?"PASS":"FAIL",(int)resp.StatusCode,null,null); }
    private async Task CaseNoToken(CancellationToken ct) { using HttpClient c=CreateHttpClient(options.ApiBaseUrl); var r=await UploadAsync(c,"XF-01-valido.csv",true,ct); AddMatrix("XF-09","sin token","HTTP 401",((int)r.Status).ToString(),r.Status==HttpStatusCode.Unauthorized?"PASS":"FAIL",(int)r.Status,null,null); }

    private async Task TraceUploadAsync(HttpClient client, string id, string uploadId, int detailCount, int errorCount, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(uploadId)) { AddMatrix(id+"-TRACE","Trazabilidad","uploadId real", "Sin uploadId", "FAIL", null, null, null); return; }
        var summary = await GetJsonAsync(client, $"/api/bulk-loads/{uploadId}", ct);
        var details = await GetJsonAsync(client, $"/api/bulk-loads/{uploadId}/details?page=1&pageSize=100", ct);
        var errors = await GetJsonAsync(client, $"/api/bulk-loads/{uploadId}/errors?page=1&pageSize=100", ct);
        var list = await GetJsonAsync(client, "/api/bulk-loads?uploadType=X_FACTOR&page=1&pageSize=20", ct);
        await WriteJsonAsync($"responses/{id}-trace.json", new JsonObject { ["summary"] = summary.DeepClone(), ["details"] = details.DeepClone(), ["errors"] = errors.DeepClone(), ["list"] = list.DeepClone() }, ct);
        bool ok = CountItems(details) == detailCount && CountItems(errors) == errorCount && ContainsText(list, uploadId);
        AddMatrix(id+"-TRACE", "Trazabilidad", $"{detailCount} detalles y {errorCount} errores", $"details={CountItems(details)}, errors={CountItems(errors)}", ok?"PASS":"FAIL", 200, uploadId, null);
    }

    private async Task ExpectFactorAsync(HttpClient client, string id, decimal expected, CancellationToken ct)
    { var current = await GetTaxClassificationAsync(client, ct); bool ok = SameFactor(current, expected) && SameBusinessFields(baseline!, current); if (!ok) AddMatrix(id+"-VERIFY", "Verificación registro", DecimalText(expected), Summ(current), "FAIL", 200, null, "AppliedFactor o campos de negocio no coinciden."); }

    private async Task<(HttpStatusCode Status, JsonNode Body)> UploadAsync(HttpClient client, string file, bool fileField, CancellationToken ct)
    { using var form = new MultipartFormDataContent(); if (fileField) { var bytes = await File.ReadAllBytesAsync(Path.Combine(csvDirectory!, file), ct); var content = new ByteArrayContent(bytes); content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/csv"); form.Add(content, "file", file); } using var resp = await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", form, ct); JsonNode body = await ReadJsonOrMessageAsync(resp, ct); return (resp.StatusCode, body); }
    private async Task<JsonNode> GetJsonAsync(HttpClient client, string path, CancellationToken ct) { using var resp = await client.GetAsync(path, ct); if (!resp.IsSuccessStatusCode) throw new SafeFailureException($"GET {path} HTTP {(int)resp.StatusCode}"); return (await resp.Content.ReadFromJsonAsync<JsonNode>(JsonOptions, ct)) ?? new JsonObject(); }
    private async Task<JsonNode?> TryWithAuthJsonAsync(string path, CancellationToken ct) { try { using HttpClient c=CreateHttpClient(options.ApiBaseUrl); var l=await LoginAsync(c,Env("NUAM_XFACTOR_TEST_EMAIL"),Env("NUAM_XFACTOR_TEST_PASSWORD"),ct); c.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue(l.TokenType,l.AccessToken); return await GetJsonAsync(c,path,ct);} catch { return null; } }
    private async Task<JsonObject?> TryWithAuthTaxClassificationAsync(CancellationToken ct) { try { using HttpClient c=CreateHttpClient(options.ApiBaseUrl); var l=await LoginAsync(c,Env("NUAM_XFACTOR_TEST_EMAIL"),Env("NUAM_XFACTOR_TEST_PASSWORD"),ct); c.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue(l.TokenType,l.AccessToken); return await GetTaxClassificationAsync(c,ct);} catch { return null; } }

    private async Task WriteCsvFilesAsync(decimal[] f, string runId, CancellationToken ct)
    {
        string no = $"NUEX-NO-EXISTE-{runId}";
        await Csv("XF-01-valido.csv", [Row(f[0])], ct); await Csv("XF-03-extension-incorrecta.txt", [Row(f[0])], ct);
        await WriteBomAsync(Path.Combine(csvDirectory!,"XF-04-encabezado-incorrecto.csv"), "bad;header\r\n1;2\r\n", ct);
        await Csv("XF-05-factor-invalido.csv", [$"{options.ExpectedMarket};{options.ExpectedInstrumentCode};{options.ExpectedTaxPeriod};texto-invalido"], ct);
        await Csv("XF-06-no-encontrado.csv", [$"{options.ExpectedMarket};{no};{options.ExpectedTaxPeriod};{f[0].ToString(CultureInfo.InvariantCulture)}"], ct);
        await Csv("XF-07-duplicado.csv", [Row(f[1]), Row(9.87654321m)], ct);
        await Csv("XF-08-mixto.csv", [Row(f[2]), $"{options.ExpectedMarket};{no}-MIXTO;{options.ExpectedTaxPeriod};{f[0].ToString(CultureInfo.InvariantCulture)}", $"{options.ExpectedMarket};{options.ExpectedInstrumentCode};{options.ExpectedTaxPeriod};texto-invalido"], ct);
        await Csv("RESTORE-factor-original.csv", [Row(GetDecimal(baseline!,"appliedFactor")!.Value)], ct);
    }
    private string Row(decimal f) => $"{options.ExpectedMarket};{options.ExpectedInstrumentCode};{options.ExpectedTaxPeriod};{DecimalText(f)}";
    private async Task Csv(string name, IEnumerable<string> rows, CancellationToken ct) => await WriteBomAsync(Path.Combine(csvDirectory!, name), Header + "\r\n" + string.Join("\r\n", rows) + "\r\n", ct);
    private static async Task WriteBomAsync(string path, string text, CancellationToken ct) => await File.WriteAllTextAsync(path, text, new UTF8Encoding(true), ct);

    private void ValidateIdentityAndFactor(JsonObject b)
    { if ((string?)b["market"] != options.ExpectedMarket || (string?)b["instrumentCode"] != options.ExpectedInstrumentCode || GetInt(b,"taxPeriod") != options.ExpectedTaxPeriod) throw new SafeFailureException("La identidad del registro no coincide con --expected-*; no se ejecutan cargas."); if (GetDecimal(b,"appliedFactor") is null) throw new SafeFailureException("appliedFactor inicial no existe o no es decimal válido; no se ejecutan cargas."); }
    private static decimal[] PickFactors(decimal baseline) { decimal[] pool=[1.23456789m,1.34567891m,1.45678912m,2.23456789m,2.34567891m]; return pool.Where(x=>x!=baseline).Take(3).ToArray(); }
    private static HttpClient CreateHttpClient(Uri apiBaseUrl) { var handler = new HttpClientHandler { AllowAutoRedirect = false }; return new HttpClient(handler) { BaseAddress = apiBaseUrl, Timeout = TimeSpan.FromSeconds(30) }; }
    private static string Env(string name) => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)) ? throw new SafeFailureException($"Falta la variable de entorno requerida: {name}") : Environment.GetEnvironmentVariable(name)!;
    private async Task<LoginResponse> LoginAsync(HttpClient client, string email, string password, CancellationToken ct) { using var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password }, JsonOptions, ct); if (!response.IsSuccessStatusCode) throw new SafeFailureException($"Login falló con HTTP {(int)response.StatusCode}"); var login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, ct); if (login is null || string.IsNullOrWhiteSpace(login.AccessToken)) throw new SafeFailureException("Login sin token válido."); return login; }
    private async Task<MeResponse> GetMeAsync(HttpClient client, CancellationToken ct) { using var response = await client.GetAsync("/api/auth/me", ct); if (!response.IsSuccessStatusCode) throw new SafeFailureException($"/api/auth/me falló con HTTP {(int)response.StatusCode}"); return await response.Content.ReadFromJsonAsync<MeResponse>(JsonOptions, ct) ?? throw new SafeFailureException("Usuario autenticado inesperado."); }
    private async Task<JsonObject> GetTaxClassificationAsync(HttpClient client, CancellationToken ct) { using var response = await client.GetAsync($"/api/tax-classifications/{options.RecordId}", ct); if (!response.IsSuccessStatusCode) throw new SafeFailureException($"Registro no disponible HTTP {(int)response.StatusCode}"); return await response.Content.ReadFromJsonAsync<JsonObject>(JsonOptions, ct) ?? throw new SafeFailureException("Registro con JSON inesperado."); }

    private static async Task<JsonNode> ReadJsonOrMessageAsync(HttpResponseMessage response, CancellationToken ct) { try { return (await response.Content.ReadFromJsonAsync<JsonNode>(JsonOptions, ct)) ?? new JsonObject(); } catch { return new JsonObject { ["httpStatus"] = (int)response.StatusCode }; } }
    private async Task SaveResponseAsync(string id, JsonNode body, CancellationToken ct) => await WriteJsonAsync($"responses/{id}.json", body, ct);
    private async Task WriteJsonAsync(string relative, JsonNode node, CancellationToken ct) { string path=Path.Combine(runDirectory!, relative); Directory.CreateDirectory(Path.GetDirectoryName(path)!); await File.WriteAllTextAsync(path, node.ToJsonString(JsonOptions), ct); }
    private async Task WriteMatrixAsync(CancellationToken ct) { var sb=new StringBuilder("caseId,caseName,expected,actual,status,httpStatus,uploadId,notes\r\n"); foreach(var r in matrix) sb.AppendJoin(',', [Q(r.CaseId),Q(r.CaseName),Q(r.Expected),Q(r.Actual),Q(r.Status),Q(r.HttpStatus?.ToString()??""),Q(r.UploadId??""),Q(r.Notes??"")]).Append("\r\n"); await WriteBomAsync(Path.Combine(runDirectory!,"test-matrix.csv"), sb.ToString(), ct); }
    private async Task WriteSummaryAsync(CancellationToken ct) { var sb=new StringBuilder($"# X Factor run\n\n- Propósito: ejecutar pruebas controladas XF-01 a XF-09 y restaurar AppliedFactor.\n- API local utilizada: {options.ApiBaseUrl}\n- Registro probado: {options.RecordId}; identidad {options.ExpectedMarket} / {options.ExpectedInstrumentCode} / {options.ExpectedTaxPeriod}\n- Factor baseline: {baselineFactor}\n- Evidencias: {runDirectory}\n- CSV: {csvDirectory}\n- Respuestas sanitizadas: {responsesDirectory}\n- Restauración: {(restorationOk?"exitosa":"fallida/no requerida antes de modificación")}\n- XF-10: NOT_EXECUTED - Cubierto por pruebas xUnit existentes; no se creó una cuenta local solo para probar permisos.\n- Confirmación: no se creó ni eliminó ninguna calificación; solo se usó la carga X Factor para modificar temporalmente AppliedFactor del registro confirmado.\n\n## Matriz\n\n"); foreach(var r in matrix) sb.AppendLine($"- {r.CaseId} | {r.CaseName} | {r.Status} | HTTP {r.HttpStatus?.ToString()??"n/a"} | {r.Notes}"); await File.WriteAllTextAsync(Path.Combine(runDirectory!,"run-summary.md"), sb.ToString(), TaxClassificationHelpers.Utf8Bom, ct); }
    private JsonObject BuildResults(DateTime s, DateTime f, JsonObject restoration) => new() { ["command"]="run", ["startedAt"]=s, ["finishedAt"]=f, ["apiBaseUrl"]=options.ApiBaseUrl.ToString(), ["recordId"]=options.RecordId, ["matrix"]=JsonSerializer.SerializeToNode(matrix, JsonOptions), ["restoration"]=restoration };
    private void AddNotExecutedCases(string note) { foreach (string id in new[]{"XF-01","XF-02","XF-03","XF-04","XF-05","XF-06","XF-07","XF-08","XF-09"}) AddMatrix(id, id, "Precondición requerida", "No ejecutado", "NOT_EXECUTED", null, null, note); AddMatrix("RESTORE", "Restauración factor original", "No requerida", "No ejecutado", "NOT_EXECUTED", null, null, "No hubo modificación porque la identidad del registro no era única."); }
    private void AddMatrix(string id,string name,string expected,string actual,string status,int? http,string? upload,string? notes) { matrix.Add(new(id,name,expected,actual,status,http,upload,notes)); Log($"{id}: {status} {actual}"); }
    private void Log(string m)=>log.Add($"{DateTime.UtcNow:O} {Safe(m)}");
    private static string Q(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
    private static string Safe(string s)=>s.Replace(Environment.GetEnvironmentVariable("NUAM_XFACTOR_TEST_EMAIL")??"\0","[redacted]").Replace(Environment.GetEnvironmentVariable("NUAM_XFACTOR_TEST_PASSWORD")??"\0","[redacted]");
    private static int? GetInt(JsonNode? n,string p)=>n?[p]?.GetValue<int>(); private static int? Int(JsonNode? n,string p)=>GetInt(n,p);
    private static string? Str(JsonNode? n,string p)=>n?[p]?.ToString();
    private static decimal? GetDecimal(JsonNode? n,string p)=> decimal.TryParse(n?[p]?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d)?d:null;
    private static string DecimalText(decimal d)=>d.ToString("0.########", CultureInfo.InvariantCulture);
    private static bool SameFactor(JsonObject o, decimal d)=>GetDecimal(o,"appliedFactor") == d;
    private static bool SameBusinessFields(JsonObject a, JsonObject b) { foreach (var f in new[]{"id","market","instrumentCode","instrumentName","classificationType","description","updatePercentage","referenceAmount","currency","taxPeriod","validFrom","validTo","status","creatorUserId","createdAt"}) if ((a[f]?.ToJsonString()??"")!=(b[f]?.ToJsonString()??"")) return false; return true; }
    private static bool ContainsUpdatedId(JsonNode? n, int id) => n?["updatedTaxClassificationIds"] is JsonArray a && a.Any(x => x?.ToString() == id.ToString(CultureInfo.InvariantCulture));
    private static bool ContainsText(JsonNode? n,string text)=>n?.ToJsonString().Contains(text,StringComparison.OrdinalIgnoreCase)==true;
    private static int CountItems(JsonNode? n)=> n?["items"] is JsonArray a ? a.Count : n is JsonArray b ? b.Count : 0;
    private static string Summ(JsonNode? n)=> n is null ? "" : n.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    private sealed record UploadResult(HttpStatusCode Status, JsonNode Body, string UploadId);
    private sealed record MatrixRow(string CaseId,string CaseName,string Expected,string Actual,string Status,int? HttpStatus,string? UploadId,string? Notes);
    private sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt);
    private sealed record MeResponse(int Id, string Role);
    private sealed class SafeFailureException(string message) : Exception(message);
}
