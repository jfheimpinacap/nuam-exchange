# Contrato API — Consulta de Cargas Masivas

## Objetivo

Exponer consultas de solo lectura para revisar la trazabilidad funcional de cargas masivas X Factor y X Monto ya ejecutadas, reutilizando exclusivamente `UploadFile`, `UploadTemplate`, `BulkUploadDetail` y `BulkUploadError`.

## Política y roles

Todos los endpoints usan `TaxClassificationRead`. Roles permitidos: `Administrador`, `Analista Tributario` y `Supervisor`.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/bulk-loads` | Historial paginado de cargas. |
| `GET` | `/api/bulk-loads/{id}` | Resumen seguro de una carga. |
| `GET` | `/api/bulk-loads/{id}/details` | Filas procesadas de una carga. |
| `GET` | `/api/bulk-loads/{id}/errors` | Errores registrados de una carga. |

No aceptan body JSON y no modifican registros.

## Paginación

`page` parte en 1. `pageSize` permite 1 a 100. La respuesta paginada incluye `items`, `page`, `pageSize`, `totalCount` y `totalPages`.

## Filtros y ordenamiento

### `GET /api/bulk-loads`

Filtros: `uploadType` (`X_FACTOR`, `X_MONTO`), `status` (`RECIBIDO`, `EN_VALIDACION`, `PROCESADO`, `PROCESADO_CON_ERRORES`, `OBSERVADO`, `RECHAZADO`), `dateFrom`, `dateTo`.

Ordenamiento permitido: `uploadedAt`, `id`, `uploadType`, `status`, `fileName`, `totalRecords`, `validRecords`, `errorRecords`. `sortDirection`: `asc` o `desc`. Por defecto ordena por `uploadedAt desc`.

### `GET /api/bulk-loads/{id}/details`

Filtros: `status` (`PENDIENTE`, `VALIDA`, `CON_ERROR`, `APLICADA`, `IGNORADA`), `affectedField`, `taxClassificationId`, `rowNumber`. Orden fijo: `rowNumber asc`, `id asc`.

### `GET /api/bulk-loads/{id}/errors`

Filtros: `severity` (`ADVERTENCIA`, `ERROR`, `CRITICO`), `column`, `rowNumber`. Orden fijo: `rowNumber asc`, `createdAt asc`, `id asc`.

## Contratos de respuesta

### Resumen de carga

Campos expuestos: `id`, `uploadType`, `fileName`, `extension`, `fileSizeBytes`, `status`, `totalRecords`, `validRecords`, `errorRecords`, `observation`, `uploadedAt`, `template`, `detailCount`, `errorCount`.

`template` expone solo `id`, `uploadType`, `templateName`, `templateVersion`, `allowedFormat` e `isActive`.

### Detalle de fila

Campos expuestos: `id`, `uploadFileId`, `taxClassificationId`, `rowNumber`, `affectedField`, `factorValue`, `amountValue`, `originalTextValue`, `rowStatus`, `observation`, `createdAt`.

### Error de carga

Campos expuestos: `id`, `uploadFileId`, `rowNumber`, `columnName`, `errorDescription`, `severity`, `createdAt`.

## Errores HTTP

- `200 OK`: consulta exitosa, incluso con colección vacía.
- `400 Bad Request`: paginación, filtro, fecha u ordenamiento inválidos.
- `401 Unauthorized`: ausencia de JWT válido.
- `404 Not Found`: carga inexistente en endpoints por id.
- `503 Service Unavailable`: falla segura de infraestructura.

## Campos no expuestos por seguridad

No se exponen `FilePath`, `FileHash`, contenido bruto completo de archivos, stack traces, connection strings, claims JWT, credenciales ni detalles de infraestructura.

## Limitaciones

No incluye descarga, reprocesamiento, eliminación, edición, frontend, reportes, auditoría general ni cambios físicos de base de datos. No crea migraciones, tablas, columnas, índices ni relaciones.
