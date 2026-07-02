namespace NuamExchange.XFactorTestRunner;

internal static class LocalExecutionGuard
{
    private static readonly HashSet<string> AllowedLocalHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "127.0.0.1",
        "::1"
    };

    public static Uri ValidateLocalApiBaseUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El argumento --api-base-url es obligatorio.");
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException("El argumento --api-base-url debe ser una URL absoluta.");
        }

        if (uri.Scheme is not "http" and not "https")
        {
            throw new ArgumentException("El argumento --api-base-url debe usar HTTP o HTTPS.");
        }

        if (!AllowedLocalHosts.Contains(uri.Host))
        {
            throw new ArgumentException(
                "El argumento --api-base-url solo puede apuntar a localhost, 127.0.0.1 o ::1. " +
                "Se rechazan dominios, IPs privadas remotas e IPs públicas.");
        }

        return uri;
    }

    public static int ValidateRecordId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El argumento --record-id es obligatorio.");
        }

        if (!int.TryParse(value, out int recordId) || recordId <= 0)
        {
            throw new ArgumentException("El argumento --record-id debe ser un entero mayor que cero.");
        }

        return recordId;
    }
}
