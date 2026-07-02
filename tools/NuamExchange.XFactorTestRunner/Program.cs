using NuamExchange.XFactorTestRunner;

const string ApplicationName = "NuamExchange X Factor Test Runner";

if (args.Length == 0 || IsHelp(args[0]))
{
    PrintHelp();
    return 0;
}

if (!string.Equals(args[0], "preflight", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Comando no reconocido.");
    PrintHelp();
    return 1;
}

if (args.Skip(1).Any(IsHelp))
{
    PrintHelp();
    return 0;
}

try
{
    RunnerOptions options = ParsePreflightOptions(args.Skip(1).ToArray());
    PrintPreflightSummary(options);
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine();
    PrintHelp();
    return 1;
}

static RunnerOptions ParsePreflightOptions(string[] args)
{
    string? apiBaseUrl = null;
    string? recordId = null;
    string? outputDirectory = null;

    for (int index = 0; index < args.Length; index++)
    {
        string argument = args[index];
        string value = ReadValue(args, ref index, argument);

        switch (argument)
        {
            case "--api-base-url":
                apiBaseUrl = value;
                break;
            case "--record-id":
                recordId = value;
                break;
            case "--output-dir":
                outputDirectory = value;
                break;
            default:
                throw new ArgumentException($"Argumento no reconocido: {argument}");
        }
    }

    Uri uri = LocalExecutionGuard.ValidateLocalApiBaseUrl(apiBaseUrl);
    int parsedRecordId = LocalExecutionGuard.ValidateRecordId(recordId);
    string outputPath = ArtifactPathResolver.Resolve(outputDirectory, Directory.GetCurrentDirectory());

    return new RunnerOptions(uri, parsedRecordId, outputPath);
}

static string ReadValue(string[] args, ref int index, string argument)
{
    if (!argument.StartsWith("--", StringComparison.Ordinal))
    {
        throw new ArgumentException($"Argumento no reconocido: {argument}");
    }

    int valueIndex = index + 1;
    if (valueIndex >= args.Length || args[valueIndex].StartsWith("--", StringComparison.Ordinal))
    {
        throw new ArgumentException($"El argumento {argument} requiere un valor.");
    }

    index = valueIndex;
    return args[valueIndex];
}

static bool IsHelp(string argument) =>
    string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase);

static void PrintPreflightSummary(RunnerOptions options)
{
    Console.WriteLine(ApplicationName);
    Console.WriteLine("Modo: preflight seguro");
    Console.WriteLine($"API local: {options.ApiBaseUrl}");
    Console.WriteLine($"Registro de prueba: {options.RecordId}");
    Console.WriteLine($"Directorio de evidencias: {options.OutputDirectory}");
    Console.WriteLine("Estado: configuración válida");
    Console.WriteLine("Sin llamadas HTTP ejecutadas.");
}

static void PrintHelp()
{
    Console.WriteLine(ApplicationName);
    Console.WriteLine();
    Console.WriteLine("Uso:");
    Console.WriteLine("  dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- preflight --api-base-url http://localhost:5000 --record-id 123");
    Console.WriteLine();
    Console.WriteLine("Comandos:");
    Console.WriteLine("  preflight                  Valida configuración local sin ejecutar llamadas HTTP.");
    Console.WriteLine();
    Console.WriteLine("Argumentos:");
    Console.WriteLine("  --api-base-url <url>       Obligatorio. URL absoluta HTTP/HTTPS local.");
    Console.WriteLine("  --record-id <int>          Obligatorio. Entero mayor que cero.");
    Console.WriteLine("  --output-dir <path>        Opcional. Carpeta externa para evidencias futuras.");
    Console.WriteLine("  --help                     Muestra esta ayuda y ejemplos.");
    Console.WriteLine();
    Console.WriteLine("Hosts permitidos: localhost, 127.0.0.1, ::1.");
    Console.WriteLine("C001 no ejecuta pruebas reales, no modifica datos y no solicita credenciales.");
}
