namespace NuamExchange.Application.TaxClassifications;

public sealed class UpdateTaxClassificationValidator : IUpdateTaxClassificationValidator
{
    public UpdateTaxClassificationValidationResult Validate(UpdateTaxClassificationCommand command)
    {
        var createValidator = new CreateTaxClassificationValidator();
        var validation = createValidator.Validate(new CreateTaxClassificationCommand(
            command.ActorUserId,
            command.Market,
            command.InstrumentCode,
            command.InstrumentName,
            command.ClassificationType,
            command.Description,
            command.UpdatePercentage,
            command.AppliedFactor,
            command.ReferenceAmount,
            command.Currency,
            command.TaxPeriod,
            command.ValidFrom,
            command.ValidTo,
            command.OriginIp));

        if (!validation.Succeeded) return UpdateTaxClassificationValidationResult.Failure(validation.Message!);

        var normalized = validation.Command!;
        return UpdateTaxClassificationValidationResult.Success(new ValidatedUpdateTaxClassificationCommand(
            command.Id,
            command.ActorUserId,
            normalized.Market,
            normalized.InstrumentCode,
            normalized.InstrumentName,
            normalized.ClassificationType,
            normalized.Description,
            normalized.UpdatePercentage,
            normalized.AppliedFactor,
            normalized.ReferenceAmount,
            normalized.Currency,
            normalized.TaxPeriod,
            normalized.ValidFrom,
            normalized.ValidTo,
            normalized.OriginIp));
    }
}
