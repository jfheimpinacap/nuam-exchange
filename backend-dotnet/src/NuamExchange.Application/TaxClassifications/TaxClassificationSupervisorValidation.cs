namespace NuamExchange.Application.TaxClassifications;

public sealed record SupervisorValidationTaxClassificationCommand(int Id, int ActorUserId, string? Decision, string? Observation, string? OriginIp);

public sealed record ValidatedSupervisorValidationTaxClassificationCommand(int Id, int ActorUserId, string Decision, string? Observation, string? OriginIp);

public sealed record SupervisorValidationResult(bool Succeeded, TaxClassificationDetailDto? Value, int StatusCode, string? Message)
{
    public static SupervisorValidationResult Success(TaxClassificationDetailDto value) => new(true, value, 200, null);
    public static SupervisorValidationResult Failure(int statusCode, string message) => new(false, null, statusCode, message);
}

public sealed record SupervisorValidationCommandValidationResult(bool Succeeded, ValidatedSupervisorValidationTaxClassificationCommand? Command, string? Message)
{
    public static SupervisorValidationCommandValidationResult Success(ValidatedSupervisorValidationTaxClassificationCommand command) => new(true, command, null);
    public static SupervisorValidationCommandValidationResult Failure(string message) => new(false, null, message);
}

public interface ISupervisorValidationTaxClassificationValidator
{
    SupervisorValidationCommandValidationResult Validate(SupervisorValidationTaxClassificationCommand command);
}
