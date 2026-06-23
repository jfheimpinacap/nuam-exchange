namespace NuamExchange.Application.Administration;

public interface IRoleManagementPolicy
{
    IReadOnlyCollection<string> ReservedRoleNames { get; }
    string NormalizeName(string? name);
    RoleNameValidationResult ValidateCustomRoleName(string? name);
    bool IsSystemRole(string? roleName);
    string ProtectedRoleMessage(string roleName);
}

public sealed record RoleNameValidationResult(bool Succeeded, string? NormalizedName, string? Message)
{
    public static RoleNameValidationResult Ok(string normalizedName) => new(true, normalizedName, null);
    public static RoleNameValidationResult Fail(string message) => new(false, null, message);
}

public sealed class DefaultRoleManagementPolicy : IRoleManagementPolicy
{
    private static readonly string[] ReservedNames = ["Administrador", "Analista Tributario", "Supervisor"];

    public IReadOnlyCollection<string> ReservedRoleNames => ReservedNames;

    public string NormalizeName(string? name) => string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();

    public RoleNameValidationResult ValidateCustomRoleName(string? name)
    {
        var normalizedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(normalizedName)) return RoleNameValidationResult.Fail("El nombre del rol es obligatorio.");
        if (normalizedName.Length > 80) return RoleNameValidationResult.Fail("El nombre del rol no puede superar 80 caracteres.");
        if (IsSystemRole(normalizedName)) return RoleNameValidationResult.Fail($"No es posible utilizar el nombre reservado {normalizedName}.");
        return RoleNameValidationResult.Ok(normalizedName);
    }

    public bool IsSystemRole(string? roleName) => ReservedNames.Any(name => string.Equals(name, NormalizeName(roleName), StringComparison.OrdinalIgnoreCase));

    public string ProtectedRoleMessage(string roleName) => $"No es posible modificar la configuración del rol base {NormalizeName(roleName)}.";
}
