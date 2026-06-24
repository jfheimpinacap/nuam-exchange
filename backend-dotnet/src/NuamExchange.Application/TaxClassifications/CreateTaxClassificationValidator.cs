namespace NuamExchange.Application.TaxClassifications;

public sealed class CreateTaxClassificationValidator : ICreateTaxClassificationValidator
{
    public const int MarketMaxLength = 120;
    public const int InstrumentCodeMaxLength = 80;
    public const int InstrumentNameMaxLength = 180;
    public const int ClassificationTypeMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int CurrencyMaxLength = 10;
    public const int OriginIpMaxLength = 60;
    public const int MinTaxPeriod = 2000;
    public const int MaxTaxPeriod = 2100;
    public const string DefaultCurrency = "CLP";

    public CreateTaxClassificationValidationResult Validate(CreateTaxClassificationCommand command)
    {
        var market = NormalizeRequired(command.Market);
        if (market is null) return CreateTaxClassificationValidationResult.Failure("El mercado es obligatorio.");
        if (market.Length > MarketMaxLength) return CreateTaxClassificationValidationResult.Failure($"El mercado no puede exceder {MarketMaxLength} caracteres.");

        var classificationType = NormalizeRequired(command.ClassificationType);
        if (classificationType is null) return CreateTaxClassificationValidationResult.Failure("El tipo de calificación es obligatorio.");
        if (classificationType.Length > ClassificationTypeMaxLength) return CreateTaxClassificationValidationResult.Failure($"El tipo de calificación no puede exceder {ClassificationTypeMaxLength} caracteres.");

        var currency = NormalizeOptional(command.Currency) ?? DefaultCurrency;
        if (currency.Length > CurrencyMaxLength) return CreateTaxClassificationValidationResult.Failure($"La moneda no puede exceder {CurrencyMaxLength} caracteres.");

        if (command.TaxPeriod is < MinTaxPeriod or > MaxTaxPeriod) return CreateTaxClassificationValidationResult.Failure("El período tributario debe estar entre 2000 y 2100.");
        if (command.ValidFrom == default) return CreateTaxClassificationValidationResult.Failure("La fecha de inicio de vigencia es obligatoria.");
        if (command.ValidTo.HasValue && command.ValidTo.Value < command.ValidFrom) return CreateTaxClassificationValidationResult.Failure("La fecha de término de vigencia no puede ser anterior a la fecha de inicio.");

        if (command.UpdatePercentage < 0) return CreateTaxClassificationValidationResult.Failure("El porcentaje de actualización no puede ser negativo.");
        if (command.AppliedFactor < 0) return CreateTaxClassificationValidationResult.Failure("El factor aplicado no puede ser negativo.");
        if (command.ReferenceAmount < 0) return CreateTaxClassificationValidationResult.Failure("El monto de referencia no puede ser negativo.");

        var instrumentCode = NormalizeOptional(command.InstrumentCode);
        if (instrumentCode?.Length > InstrumentCodeMaxLength) return CreateTaxClassificationValidationResult.Failure($"El código de instrumento no puede exceder {InstrumentCodeMaxLength} caracteres.");
        var instrumentName = NormalizeOptional(command.InstrumentName);
        if (instrumentName?.Length > InstrumentNameMaxLength) return CreateTaxClassificationValidationResult.Failure($"El nombre de instrumento no puede exceder {InstrumentNameMaxLength} caracteres.");
        var description = NormalizeOptional(command.Description);
        if (description?.Length > DescriptionMaxLength) return CreateTaxClassificationValidationResult.Failure($"La descripción no puede exceder {DescriptionMaxLength} caracteres.");
        var originIp = NormalizeOptional(command.OriginIp);
        if (originIp?.Length > OriginIpMaxLength) originIp = originIp[..OriginIpMaxLength];

        return CreateTaxClassificationValidationResult.Success(new ValidatedCreateTaxClassificationCommand(command.CreatorUserId, market, instrumentCode, instrumentName, classificationType, description, command.UpdatePercentage, command.AppliedFactor, command.ReferenceAmount, currency, command.TaxPeriod, command.ValidFrom, command.ValidTo, originIp));
    }

    private static string? NormalizeRequired(string? value)
    {
        var normalized = NormalizeOptional(value);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }
}
