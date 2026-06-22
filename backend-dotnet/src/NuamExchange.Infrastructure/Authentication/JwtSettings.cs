namespace NuamExchange.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "NuamExchange.Api";
    public string Audience { get; set; } = "NuamExchange.Frontend";
    public int AccessTokenMinutes { get; set; } = 30;
    public string SigningKey { get; set; } = string.Empty;
}
