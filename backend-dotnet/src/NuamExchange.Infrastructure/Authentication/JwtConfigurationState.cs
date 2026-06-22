using System.Text;
using Microsoft.Extensions.Options;

namespace NuamExchange.Infrastructure.Authentication;

public sealed class JwtConfigurationState(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _settings = options.Value;

    public bool IsConfigured => GetValidationError() is null;

    public string? GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(_settings.Issuer) || string.IsNullOrWhiteSpace(_settings.Audience))
        {
            return "Issuer y Audience JWT son obligatorios.";
        }

        if (string.IsNullOrWhiteSpace(_settings.SigningKey) || Encoding.UTF8.GetByteCount(_settings.SigningKey) < 32)
        {
            return "La llave de firma JWT no está configurada para este entorno.";
        }

        return null;
    }
}
