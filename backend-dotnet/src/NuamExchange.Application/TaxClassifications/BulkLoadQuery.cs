namespace NuamExchange.Application.TaxClassifications;

public sealed record BulkLoadQuery(int? Page = null, int? PageSize = null, string? UploadType = null, string? Status = null, DateTime? DateFrom = null, DateTime? DateTo = null, string? SortBy = null, string? SortDirection = null);
public sealed record BulkLoadDetailQuery(int? Page = null, int? PageSize = null, string? Status = null, string? AffectedField = null, int? TaxClassificationId = null, int? RowNumber = null);
public sealed record BulkLoadErrorQuery(int? Page = null, int? PageSize = null, string? Severity = null, string? Column = null, int? RowNumber = null);

public sealed record ValidatedBulkLoadQuery(int Page, int PageSize, string? UploadType, string? Status, DateTime? DateFrom, DateTime? DateTo, string SortBy, string SortDirection);
public sealed record ValidatedBulkLoadDetailQuery(int Page, int PageSize, string? Status, string? AffectedField, int? TaxClassificationId, int? RowNumber);
public sealed record ValidatedBulkLoadErrorQuery(int Page, int PageSize, string? Severity, string? Column, int? RowNumber);
public sealed record BulkLoadQueryValidationResult<T>(bool Succeeded, T? Query, string? Message)
{
    public static BulkLoadQueryValidationResult<T> Success(T query) => new(true, query, null);
    public static BulkLoadQueryValidationResult<T> Failure(string message) => new(false, default, message);
}

public interface IBulkLoadQueryValidator
{
    BulkLoadQueryValidationResult<ValidatedBulkLoadQuery> Validate(BulkLoadQuery query);
    BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery> ValidateDetails(BulkLoadDetailQuery query);
    BulkLoadQueryValidationResult<ValidatedBulkLoadErrorQuery> ValidateErrors(BulkLoadErrorQuery query);
}

public sealed class BulkLoadQueryValidator : IBulkLoadQueryValidator
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    private static readonly HashSet<string> UploadTypes = new(StringComparer.OrdinalIgnoreCase) { "X_FACTOR", "X_MONTO" };
    private static readonly HashSet<string> UploadStatuses = new(StringComparer.OrdinalIgnoreCase) { "RECIBIDO", "EN_VALIDACION", "PROCESADO", "PROCESADO_CON_ERRORES", "OBSERVADO", "RECHAZADO" };
    private static readonly HashSet<string> RowStatuses = new(StringComparer.OrdinalIgnoreCase) { "PENDIENTE", "VALIDA", "CON_ERROR", "APLICADA", "IGNORADA" };
    private static readonly HashSet<string> Severities = new(StringComparer.OrdinalIgnoreCase) { "ADVERTENCIA", "ERROR", "CRITICO" };
    private static readonly HashSet<string> SortFields = new(StringComparer.OrdinalIgnoreCase) { "uploadedAt", "id", "uploadType", "status", "fileName", "totalRecords", "validRecords", "errorRecords" };

    public BulkLoadQueryValidationResult<ValidatedBulkLoadQuery> Validate(BulkLoadQuery query)
    {
        var pageResult = ValidatePage(query.Page, query.PageSize); if (pageResult.Message is not null) return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Failure(pageResult.Message);
        var uploadType = Normalize(query.UploadType); if (uploadType is not null && !UploadTypes.Contains(uploadType)) return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Failure("El tipo de carga no es válido.");
        var status = Normalize(query.Status); if (status is not null && !UploadStatuses.Contains(status)) return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Failure("El estado de carga no es válido.");
        if (query.DateFrom is not null && query.DateTo is not null && query.DateFrom > query.DateTo) return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Failure("La fecha desde no puede ser posterior a la fecha hasta.");
        var sortBy = Normalize(query.SortBy) ?? "uploadedAt"; if (!SortFields.Contains(sortBy)) return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Failure("El campo de ordenamiento no está permitido.");
        var direction = Normalize(query.SortDirection)?.ToLowerInvariant() ?? "desc"; if (direction is not ("asc" or "desc")) return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Failure("La dirección de ordenamiento debe ser asc o desc.");
        return BulkLoadQueryValidationResult<ValidatedBulkLoadQuery>.Success(new(pageResult.Page, pageResult.PageSize, uploadType, status, query.DateFrom, query.DateTo, sortBy, direction));
    }
    public BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery> ValidateDetails(BulkLoadDetailQuery query)
    {
        var pageResult = ValidatePage(query.Page, query.PageSize); if (pageResult.Message is not null) return BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery>.Failure(pageResult.Message);
        var status = Normalize(query.Status); if (status is not null && !RowStatuses.Contains(status)) return BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery>.Failure("El estado de fila no es válido.");
        if (query.TaxClassificationId is <= 0) return BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery>.Failure("El identificador de calificación debe ser mayor que cero.");
        if (query.RowNumber is <= 0) return BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery>.Failure("El número de fila debe ser mayor que cero.");
        return BulkLoadQueryValidationResult<ValidatedBulkLoadDetailQuery>.Success(new(pageResult.Page, pageResult.PageSize, status, Normalize(query.AffectedField), query.TaxClassificationId, query.RowNumber));
    }
    public BulkLoadQueryValidationResult<ValidatedBulkLoadErrorQuery> ValidateErrors(BulkLoadErrorQuery query)
    {
        var pageResult = ValidatePage(query.Page, query.PageSize); if (pageResult.Message is not null) return BulkLoadQueryValidationResult<ValidatedBulkLoadErrorQuery>.Failure(pageResult.Message);
        var severity = Normalize(query.Severity); if (severity is not null && !Severities.Contains(severity)) return BulkLoadQueryValidationResult<ValidatedBulkLoadErrorQuery>.Failure("La severidad no es válida.");
        if (query.RowNumber is <= 0) return BulkLoadQueryValidationResult<ValidatedBulkLoadErrorQuery>.Failure("El número de fila debe ser mayor que cero.");
        return BulkLoadQueryValidationResult<ValidatedBulkLoadErrorQuery>.Success(new(pageResult.Page, pageResult.PageSize, severity, Normalize(query.Column), query.RowNumber));
    }
    private static (int Page, int PageSize, string? Message) ValidatePage(int? page, int? pageSize)
    {
        var p = page ?? DefaultPage; var ps = pageSize ?? DefaultPageSize;
        if (p < 1) return (p, ps, "La página debe ser mayor que cero.");
        if (ps < 1 || ps > MaxPageSize) return (p, ps, $"El tamaño de página debe estar entre 1 y {MaxPageSize}.");
        return (p, ps, null);
    }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
