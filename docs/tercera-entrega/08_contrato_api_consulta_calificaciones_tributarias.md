# 08 — Contrato API de consulta de Calificaciones Tributarias

## Objetivo

Implementar una API de solo lectura para consultar calificaciones tributarias existentes. El módulo permite listado, filtros disponibles según el modelo físico vigente, paginación, ordenamiento seguro, detalle por identificador y opciones de filtros derivadas desde la base de datos.

## Roles con acceso

La política `TaxClassificationRead` permite acceso a:

- `Administrador`.
- `Analista Tributario`.
- `Supervisor`.

No se usa autorización dinámica por permisos en este contrato.

## Endpoints

| Método | Ruta | Propósito |
| --- | --- | --- |
| GET | `/api/tax-classifications` | Listar calificaciones tributarias. |
| GET | `/api/tax-classifications/{id}` | Consultar detalle seguro. |
| GET | `/api/tax-classifications/filter-options` | Obtener opciones reales de filtros. |

## Filtros realmente disponibles

La entidad `TaxClassification` contiene equivalentes reales para:

- `market` → `Market`.
- `exercise` → `TaxPeriod`.
- `status` → `Status`.
- `search` → búsqueda sobre `InstrumentCode`, `InstrumentName`, `Description` y `ClassificationType`.

El filtro `origin` se omite porque no existe una propiedad equivalente en `TaxClassification`.

## Campos devueltos por listado

Cada elemento de listado devuelve solo campos reales de `TaxClassification`:

- `id`.
- `market`.
- `instrumentCode`.
- `instrumentName`.
- `classificationType`.
- `description`.
- `updatePercentage`.
- `appliedFactor`.
- `referenceAmount`.
- `currency`.
- `taxPeriod`.
- `validFrom`.
- `validTo`.
- `status`.

## Campos devueltos por detalle

El detalle devuelve los campos anteriores más:

- `creatorUserId`.
- `createdAt`.
- `updatedAt`.

No incluye historial, validaciones, archivos de carga ni relaciones internas.

## Paginación

- `page` por defecto: `1`.
- `pageSize` por defecto: `20`.
- `pageSize` máximo: `100`.
- `page < 1` responde `400`.
- `pageSize < 1` o `pageSize > 100` responde `400`.

## Ordenamiento permitido

`sortDirection` acepta solo `asc` o `desc`.

`sortBy` usa lista blanca:

- `id`.
- `market`.
- `instrumentCode`.
- `instrumentName`.
- `classificationType`.
- `currency`.
- `taxPeriod`.
- `validFrom`.
- `validTo`.
- `status`.
- `createdAt`.
- `updatedAt`.

El orden por defecto es estable: `validFrom desc` con desempate por `id desc`.

## Opciones de filtro

`GET /api/tax-classifications/filter-options` retorna valores existentes desde base de datos, sin hardcodear:

```json
{
  "markets": [],
  "exercises": [],
  "statuses": []
}
```

No retorna `origins` porque no existe `Origin` ni equivalente físico en la entidad.

## Códigos HTTP

- `200`: consulta exitosa.
- `400`: parámetros inválidos.
- `401`: JWT ausente o inválido.
- `403`: rol no autorizado.
- `404`: detalle inexistente.
- `503`: base de datos no disponible, sin exponer detalles técnicos.

## Limitaciones del prompt

No se implementa creación, edición, copia, eliminación, historial, validación supervisora, aprobación, rechazo, cargas masivas ni frontend. No se modificó el modelo físico, no se crearon migraciones y no se agregaron datos semilla.
