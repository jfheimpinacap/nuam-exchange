namespace NuamExchange.Application.TaxAudits;

public static class TaxAuditRules
{
    public const string TaxClassificationEntity = "CalificacionTributaria";
    public static readonly string[] AllowedActionValues =
    [
        "TAX_CLASSIFICATION_CREATED",
        "TAX_CLASSIFICATION_UPDATED",
        "TAX_CLASSIFICATION_COPIED",
        "TAX_CLASSIFICATION_VALIDATED",
        "TAX_CLASSIFICATION_FACTOR_BULK_UPDATED",
        "TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED"
    ];
    public static readonly IReadOnlySet<string> AllowedActions = new HashSet<string>(AllowedActionValues, StringComparer.Ordinal);
    public static readonly IReadOnlySet<string> AllowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id", "action", "taxClassificationId", "actorUserId", "occurredAt" };
}
