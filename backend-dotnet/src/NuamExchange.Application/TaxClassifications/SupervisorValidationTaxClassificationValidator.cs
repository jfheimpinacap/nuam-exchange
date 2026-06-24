namespace NuamExchange.Application.TaxClassifications;

public sealed class SupervisorValidationTaxClassificationValidator : ISupervisorValidationTaxClassificationValidator
{
    public const int ObservationMaxLength = 700;
    public const int OriginIpMaxLength = 60;
    private static readonly IReadOnlySet<string> AllowedDecisions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "VALIDADO", "OBSERVADO", "APROBADO" };

    public SupervisorValidationCommandValidationResult Validate(SupervisorValidationTaxClassificationCommand command)
    {
        var decision = Normalize(command.Decision);
        if (decision is null) return SupervisorValidationCommandValidationResult.Failure("La decisión de validación es obligatoria.");
        if (!AllowedDecisions.Contains(decision)) return SupervisorValidationCommandValidationResult.Failure("La decisión de validación no está permitida.");

        var observation = Normalize(command.Observation);
        if (observation?.Length > ObservationMaxLength) return SupervisorValidationCommandValidationResult.Failure($"La observación no puede exceder {ObservationMaxLength} caracteres.");

        var originIp = Normalize(command.OriginIp);
        if (originIp?.Length > OriginIpMaxLength) originIp = originIp[..OriginIpMaxLength];

        return SupervisorValidationCommandValidationResult.Success(new ValidatedSupervisorValidationTaxClassificationCommand(command.Id, command.ActorUserId, decision.ToUpperInvariant(), observation, originIp));
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
