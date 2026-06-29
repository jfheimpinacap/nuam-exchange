# 19. Contrato API de consulta segura de metadatos de respaldos

## Propósito

La API interna `GET /api/backup-metadata` permite consultar metadatos mínimos de registros existentes en la tabla `Respaldo`. Su propósito es entregar visibilidad administrativa limitada sobre metadatos parciales, sin afirmar existencia física, integridad ni recuperabilidad de respaldos.

## Alcance estricto de solo lectura

La API es estrictamente de consulta. No ejecuta backup, no ejecuta restore, no descarga, no sube, no crea archivos, no accede a rutas, no automatiza procesos y no modifica datos.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/backup-metadata` | Lista metadatos seguros, filtrados, ordenados y paginados. |
| `GET` | `/api/backup-metadata/{id}` | Obtiene el detalle seguro de un registro existente o responde `404`. |

No existen endpoints `POST`, `PUT`, `PATCH`, `DELETE`, download, restore, execute, trigger ni retry para este contrato.

## Política de autorización

Ambos endpoints usan la política `BackupMetadataRead`, registrada exclusivamente para el rol real `Administrador`.

| Actor | Resultado |
|---|---|
| Administrador autenticado | Permitido (`200` si la consulta es válida). |
| Supervisor autenticado | Prohibido (`403 Forbidden`). |
| Analista Tributario autenticado | Prohibido (`403 Forbidden`). |
| Otro rol autenticado | Prohibido (`403 Forbidden`). |
| No autenticado | Prohibido (`401 Unauthorized`). |

No hay autorización para backup ni restore real.

## Parámetros de listado

| Parámetro | Regla |
|---|---|
| `backupType` | Opcional. Valor exacto permitido por el modelo: `BASE_DATOS`, `ARCHIVOS`, `COMPLETO`. |
| `status` | Opcional. Valor exacto permitido por el modelo: `PROGRAMADO`, `EJECUTADO`, `FALLIDO`, `RESTAURADO`. |
| `dateFrom` | Opcional. Fecha/hora mínima de `occurredAt`. |
| `dateTo` | Opcional. Fecha/hora máxima de `occurredAt`; no puede ser anterior a `dateFrom`. |
| `sortBy` | Opcional. Lista cerrada: `occurredAt`, `id`, `backupType`, `status`. |
| `sortDirection` | Opcional. Solo `asc` o `desc`. |
| `page` | Opcional. Mínimo `1`. |
| `pageSize` | Opcional. Mínimo `1`, máximo `100`. |

El orden por defecto es `occurredAt desc` y luego `id desc`.

## Respuestas seguras

Listado:

```json
{
  "items": [
    {
      "id": 10,
      "backupType": "BASE_DATOS",
      "status": "EJECUTADO",
      "occurredAt": "2026-06-03T12:00:00Z",
      "hasObservation": true
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

Detalle:

```json
{
  "id": 10,
  "backupType": "BASE_DATOS",
  "status": "EJECUTADO",
  "occurredAt": "2026-06-03T12:00:00Z",
  "hasObservation": true
}
```

## Campos explícitamente excluidos

La API no expone `ruta_respaldo`, `BackupPath`, rutas completas o parciales, carpetas, archivos, extensiones, unidades locales o de red, URI, hash, tamaño, contenido, `observacion`, `Observation`, usuario, `usuario_id`, nombres, correos, roles, connection strings, secretos, credenciales, tokens ni datos de infraestructura.

## Respuestas HTTP esperadas

| Código | Uso |
|---|---|
| `200 OK` | Consulta válida autorizada. |
| `400 Bad Request` | Parámetros inválidos, `id` no positivo o rango de fechas inválido. |
| `401 Unauthorized` | Usuario no autenticado. |
| `403 Forbidden` | Usuario autenticado sin rol `Administrador`. |
| `404 Not Found` | Registro inexistente o fuera del alcance permitido. |
| `503 Service Unavailable` | Solo ante indisponibilidad de infraestructura siguiendo la convención existente de consultas. |

## Confirmaciones negativas

- No ejecuta `BACKUP DATABASE`.
- No ejecuta `RESTORE DATABASE`.
- No accede a archivos ni rutas.
- No crea auditorías.
- No crea registros de `Respaldo`.
- No ejecuta migraciones ni cambios físicos de base de datos.

## Limitaciones conocidas

Los datos consultados son metadatos parciales. Un registro en `Respaldo` no prueba que exista un archivo físico, que sea íntegro, que sea accesible ni que sea recuperable.

## Ejemplos seguros

Request:

```http
GET /api/backup-metadata?backupType=BASE_DATOS&status=EJECUTADO&page=1&pageSize=10
Authorization: Bearer <token-administrador>
```

Response segura:

```json
{
  "items": [
    {
      "id": 25,
      "backupType": "BASE_DATOS",
      "status": "EJECUTADO",
      "occurredAt": "2026-06-01T10:30:00Z",
      "hasObservation": false
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```
