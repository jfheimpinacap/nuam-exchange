# 13 — Contrato API Carga Masiva X Factor

## Objetivo

Implementar la primera versión síncrona de Carga Masiva X Factor para actualizar exclusivamente `AppliedFactor` en calificaciones tributarias existentes, con trazabilidad en `ArchivoCarga`, `DetalleCargaMasiva`, `ErrorCargaMasiva`, `ClassificationHistory` y `AuditLog`.

## Alcance exacto

- Endpoint: `POST /api/tax-classifications/bulk-loads/x-factor`.
- Política: `TaxClassificationWrite`.
- Roles permitidos: `Administrador` y `Analista Tributario`.
- Roles denegados: `Supervisor` y cualquier rol que no cumpla `TaxClassificationWrite`.
- No crea calificaciones tributarias.
- No cambia estados ni ejecuta validación supervisora.

## Contrato multipart/form-data

El request debe ser `multipart/form-data` y debe incluir obligatoriamente el campo:

```text
file
```

No existe contrato JSON alternativo para reemplazar el archivo.

## Archivo permitido

- Extensión: `.csv`.
- Codificación: UTF-8 válida, con o sin BOM.
- Delimitador: punto y coma (`;`).
- Encabezado obligatorio, en este orden lógico y aceptado sin distinción de mayúsculas/minúsculas:

```text
market;instrumentCode;taxPeriod;appliedFactor
```

Ejemplo:

```csv
market;instrumentCode;taxPeriod;appliedFactor
BOLSA;NUEX-PRUEBA-001;2026;1.25000000
BOLSA;NUEX-PRUEBA-002;2026;0.87500000
```

## Identidad lógica

Cada fila identifica la calificación tributaria con la combinación exacta, luego de `Trim` seguro:

```text
market + instrumentCode + taxPeriod
```

No se usa `instrumentCode` como identificador global único.

## Campo actualizado

Solo se actualizan:

- `AppliedFactor`.
- `UpdatedAt`, usando timestamp de servidor.

Nunca se modifican por esta carga: `Market`, `InstrumentCode`, `InstrumentName`, `ClassificationType`, `Description`, `UpdatePercentage`, `ReferenceAmount`, `Currency`, `TaxPeriod`, `ValidFrom`, `ValidTo`, `Status`, `CreatorUserId`, `CreatedAt`.

## Tratamiento de filas

- Fila válida con coincidencia única: actualiza `AppliedFactor`, registra detalle `APLICADA`, historial `MODIFICACION` y auditoría `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED`.
- Fila con coincidencia inexistente: registra detalle `CON_ERROR` y `ErrorCargaMasiva`; no modifica calificación.
- Fila con coincidencia ambigua: registra detalle `CON_ERROR` y `ErrorCargaMasiva`; no modifica calificación.
- Decimal inválido o incompatible con `decimal(18,8)` / no negativo: registra error; no modifica calificación.
- Identidad duplicada dentro del mismo archivo: la primera ocurrencia válida puede procesarse; ocurrencias posteriores quedan con error de duplicidad.
- Filas válidas e inválidas pueden coexistir si la estructura global del archivo es válida.

## Respuesta 200 OK

La respuesta usa un contrato explícito:

```json
{
  "uploadId": 10,
  "totalRows": 2,
  "successfulRows": 1,
  "failedRows": 1,
  "updatedTaxClassificationIds": [7],
  "errors": [
    { "rowNumber": 3, "code": "NOT_FOUND", "message": "No existe una calificación tributaria para market + instrumentCode + taxPeriod." }
  ]
}
```

## Errores HTTP

- `400 Bad Request`: falta `file`, archivo vacío, extensión no CSV, UTF-8 inválido, encabezado inválido o estructura global inválida.
- `401 Unauthorized`: falta JWT válido.
- `403 Forbidden`: Supervisor u otro rol sin `TaxClassificationWrite`.
- `503 Service Unavailable`: falla segura de infraestructura o base de datos, sin stack trace ni secretos.

## Trazabilidad

- `ArchivoCarga`: registra `X_FACTOR`, `CSV`, nombre, tamaño, usuario, contadores y estado final `PROCESADO` o `PROCESADO_CON_ERRORES`.
- `DetalleCargaMasiva`: registra una fila por fila de datos, con `AppliedFactor`, texto original acotado y estado `APLICADA` o `CON_ERROR`.
- `ErrorCargaMasiva`: registra errores de fila con severidad `ERROR`.
- `ClassificationHistory`: se registra solo para filas actualizadas con `tipo_cambio = MODIFICACION`, `ModifiedField = AppliedFactor`, valor anterior y nuevo.
- `AuditLog`: se registra solo para filas actualizadas con acción `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED`.

## Consistencia transaccional

La carga se procesa en una transacción EF Core. Una fila inválida no bloquea otras filas válidas, pero una falla inesperada revierte la operación completa y evita persistencia parcial.

## Limitaciones

- Sin Carga X Monto.
- Sin creación masiva.
- Sin XLSX/XLS/PDF/TXT/JSON/XML/ZIP.
- Sin frontend.
- Sin consulta histórica completa de cargas.
- Sin cambios de estado.
- Sin almacenamiento cloud ni plantillas descargables.
