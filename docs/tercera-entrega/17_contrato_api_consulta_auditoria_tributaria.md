# Contrato API — Consulta segura de auditoría tributaria

## Objetivo
Permitir la revisión de solo lectura de eventos de `Auditoria` asociados a operaciones tributarias reales sobre calificaciones tributarias.

## Alcance tributario y regla exacta de inclusión
Un evento se considera tributario solo si cumple simultáneamente:

1. `entidad_afectada` / `AffectedEntity` es exactamente `CalificacionTributaria`.
2. `registro_afectado_id` / `AffectedRecordId` no es nulo.
3. `accion` / `Action` pertenece a la lista cerrada:
   - `TAX_CLASSIFICATION_CREATED`.
   - `TAX_CLASSIFICATION_UPDATED`.
   - `TAX_CLASSIFICATION_COPIED`.
   - `TAX_CLASSIFICATION_VALIDATED`.
   - `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED`.
   - `TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED`.

No se usan búsquedas difusas ni prefijos abiertos.

## Endpoints
- `GET /api/tax-audits`.
- `GET /api/tax-audits/{id}`.

Ambos usan `TaxClassificationRead`.

## Roles permitidos
- Administrador.
- Analista Tributario.
- Supervisor.

## Paginación, filtros y ordenamiento
`GET /api/tax-audits` acepta `page`, `pageSize`, `taxClassificationId`, `action`, `dateFrom`, `dateTo`, `sortBy` y `sortDirection`.

- `page >= 1`.
- `pageSize` entre 1 y 100.
- `dateFrom <= dateTo`.
- `action` debe estar en la lista cerrada tributaria.
- `sortDirection`: `asc` o `desc`.
- `sortBy`: `id`, `action`, `taxClassificationId`, `actorUserId`, `occurredAt`.
- Orden por defecto: `occurredAt desc`, `id desc`.

La respuesta paginada incluye `items`, `page`, `pageSize`, `totalCount` y `totalPages`. Sin resultados devuelve `200 OK`, `items` vacío no nulo y `totalPages = 0`.

## Respuestas HTTP
- `200 OK`: consulta exitosa.
- `400 Bad Request`: parámetros inválidos.
- `401 Unauthorized`: sin JWT válido.
- `404 Not Found`: id inexistente o existente fuera del alcance tributario.
- `503 Service Unavailable`: falla segura de infraestructura.

## Campos seguros expuestos
Listado: `id`, `action`, `taxClassificationId`, `actorUserId`, `occurredAt`.

Detalle: además de los campos anteriores, `detail`, `previousValue` y `newValue`, solo dentro de la regla tributaria cerrada.

## Campos excluidos
No se expone `OriginIp` / `ip_origen`, rutas, credenciales, JWT, claims, emails, nombres de usuario, roles, connection strings, hashes, stack traces ni detalles de infraestructura.

## Tratamiento de evento inexistente y no tributario
Un id inexistente responde `404 Not Found`. Un id existente pero no tributario también responde `404 Not Found` para no revelar auditoría ajena.

## Solo lectura y limitaciones
La consulta usa `AsNoTracking`, no crea auditoría nueva y no modifica `AuditLog`, `TaxClassification`, `ClassificationHistory`, `UploadFile`, `BulkUploadDetail`, `BulkUploadError` ni `TaxReport`.

Limitaciones: sin consulta genérica de auditoría, sin auditoría de usuarios, sin auditoría de autenticación, sin auditoría de infraestructura, sin exportación, sin frontend y sin cambios físicos de base de datos.
