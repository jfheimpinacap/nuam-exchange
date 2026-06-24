# 16. Contrato API — Reporte Tributario de Calificaciones y Exportación CSV

## Objetivo y alcance

Implementar una consulta de solo lectura sobre `CalificacionTributaria` y una exportación CSV segura del mismo universo filtrado. No calcula impuestos, retenciones, pagos, montos netos ni resultados financieros.

## Fuente de datos

La fuente es la entidad `TaxClassification` / tabla `CalificacionTributaria`. Se inspeccionó `TaxReport` / `ReporteTributario`, pero no se usa para persistir esta exportación porque su semántica histórica no está implementada como bitácora compatible para este flujo en tiempo real.

## Endpoints

### `GET /api/tax-reports/tax-classifications`

Política: `TaxClassificationRead`. Roles permitidos: `Administrador`, `Analista Tributario`, `Supervisor`.

Parámetros: `page`, `pageSize`, `market`, `instrumentCode`, `taxPeriod`, `status`, `classificationType`, `currency`, `sortBy`, `sortDirection`.

Paginación: `page >= 1`, `pageSize` entre 1 y 100. Ordenamiento permitido: `id`, `market`, `instrumentCode`, `instrumentName`, `classificationType`, `taxPeriod`, `status`, `appliedFactor`, `referenceAmount`, `currency`, `validFrom`, `validTo`, `updatedAt`. Dirección: `asc` o `desc`.

La respuesta incluye `generatedAt`, `appliedFilters`, `summary`, `items`, `page`, `pageSize`, `totalCount` y `totalPages`.

Resumen agregado: `totalClassifications`, `countByStatus`, `countByClassificationType`, `countWithAppliedFactor`, `countWithReferenceAmount` y `referenceAmountTotalsByCurrency`. Los montos de referencia se agrupan por moneda; los `null` no se suman ni se cuentan como monto.

### `GET /api/tax-reports/tax-classifications/export`

Aplica los mismos filtros y ordenamiento funcionales, sin paginación. Límite máximo: 10000 filas. Si el total filtrado supera el límite, responde `400 Bad Request` y no genera archivo parcial.

CSV: UTF-8 con BOM, `Content-Type: text/csv; charset=utf-8`, `Content-Disposition: attachment`, nombre `reporte_calificaciones_tributarias_YYYYMMDD_HHMMSS.csv`, delimitador `;`.

Encabezados: `market;instrumentCode;instrumentName;classificationType;taxPeriod;status;appliedFactor;referenceAmount;currency;validFrom;validTo;updatedAt`.

Escape: comillas dobles duplicadas, campos con `;`, comillas o saltos de línea entre comillas. Decimales con cultura invariante y fechas ISO. `null` se representa como vacío. Formula Injection: textos que comienzan con `=`, `+`, `-` o `@` se neutralizan anteponiendo apóstrofo; no se alteran números ni fechas.

## Respuestas HTTP

- `200 OK` para consulta o archivo CSV válido, incluido CSV solo con encabezado cuando no hay filas.
- `400 Bad Request` para filtros inválidos, ordenamiento inválido o límite excedido.
- `401 Unauthorized` sin JWT.
- `503 Service Unavailable` ante falla segura de infraestructura.

## Campos no expuestos y solo lectura

No expone credenciales, claims, stack traces, connection strings, rutas locales, hashes, `FilePath`, `FileHash` ni campos internos de infraestructura. No crea ni modifica `TaxClassification`, `UploadFile`, `BulkUploadDetail`, `BulkUploadError`, `ClassificationHistory`, `AuditLog` ni `ReporteTributario`.

## Limitaciones

Sin cálculo de impuestos, sin creación de reportes persistentes, sin XLSX, sin PDF, sin frontend, sin modificaciones tributarias y sin cambios físicos de base de datos.
