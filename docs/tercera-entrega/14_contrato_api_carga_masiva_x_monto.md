# Contrato API — Carga Masiva X Monto

## Objetivo
Permitir que usuarios con permiso de escritura actualicen masivamente el monto de referencia (`ReferenceAmount` / `monto_referencia`) de calificaciones tributarias existentes.

## Alcance
- Endpoint: `POST /api/tax-classifications/bulk-loads/x-amount`.
- Política: `TaxClassificationWrite`.
- Roles permitidos: Administrador y Analista Tributario.
- Roles denegados: Supervisor y cualquier rol sin escritura.
- No crea calificaciones tributarias.
- No actualiza X Factor ni otros campos tributarios.

## Contrato multipart/form-data
La solicitud debe enviarse como `multipart/form-data` con un único archivo en el campo obligatorio `file`. No existe alternativa JSON para reemplazar el archivo.

## CSV
- Codificación: UTF-8 con o sin BOM.
- Delimitador: punto y coma (`;`).
- Encabezado obligatorio, comparado sin distinguir mayúsculas/minúsculas:

```csv
market;instrumentCode;taxPeriod;referenceAmount
```

Ejemplo:

```csv
market;instrumentCode;taxPeriod;referenceAmount
BOLSA;NUEX-PRUEBA-001;2026;1500000.0000
BOLSA;NUEX-PRUEBA-002;2026;2500000.5000
```

## Identidad lógica
Cada fila identifica una calificación tributaria por `market + instrumentCode + taxPeriod`. Si no existe coincidencia se registra `NOT_FOUND`; si existe más de una coincidencia se registra `AMBIGUOUS_MATCH`.

## Campo actualizado y campos preservados
Una fila válida actualiza exclusivamente `ReferenceAmount` y `UpdatedAt`. Se preservan `Market`, `InstrumentCode`, `InstrumentName`, `ClassificationType`, `Description`, `UpdatePercentage`, `AppliedFactor`, `Currency`, `TaxPeriod`, `ValidFrom`, `ValidTo`, `Status`, `CreatorUserId` y `CreatedAt`.

## Validación de monto
`referenceAmount` es obligatorio en el contrato de carga. Debe ser decimal válido, no negativo y compatible con `decimal(18,4)`, alineado con el CHECK físico `monto_referencia IS NULL OR monto_referencia >= 0`. Se acepta formato invariante con punto y formato `es-CL` con coma decimal mediante parseo controlado.

## Filas válidas, inválidas, inexistentes, ambiguas y duplicadas
- Fila válida con coincidencia única: se aplica y registra detalle `APLICADA`.
- Fila inválida: no modifica la calificación y registra detalle `CON_ERROR` más `BulkUploadError`.
- Inexistente: `NOT_FOUND`.
- Ambigua: `AMBIGUOUS_MATCH`.
- Duplicada: `DUPLICATE_ROW` solo si una fila anterior con la misma identidad ya actualizó correctamente.
- Una fila inválida no consume la identidad; una fila válida posterior con la misma identidad puede procesarse.

## Respuestas HTTP
- `200 OK`: archivo estructuralmente válido procesado, con conteos y errores de fila seguros.
- `400 Bad Request`: falta `file`, archivo vacío, no CSV, UTF-8 inválido, encabezado inválido o estructura global inválida.
- `401 Unauthorized`: falta JWT o no se identifica el usuario autenticado.
- `403 Forbidden`: rol sin `TaxClassificationWrite`, incluido Supervisor.
- `503 Service Unavailable`: falla segura de infraestructura, sin stack trace ni secretos.

## Trazabilidad
- `UploadTemplate`: plantilla lógica `X_MONTO` versión `1.0` con columnas requeridas del CSV.
- `UploadFile`: archivo lógico recibido, tipo `X_MONTO`, extensión `CSV`, estado y contadores.
- `BulkUploadDetail`: una fila por registro procesado, `AffectedField = ReferenceAmount`, `AmountValue` cuando aplica y estado permitido.
- `BulkUploadError`: errores de fila con número de fila, columna cuando corresponde y severidad `ERROR`.

## Historial y auditoría
Cada fila aplicada crea historial `MODIFICACION` con `ModifiedField = ReferenceAmount`, valor anterior y nuevo. También crea auditoría `TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED` sobre `CalificacionTributaria` con detalle breve.

## Consistencia transaccional
La operación usa transacción EF Core. Errores normales de fila no bloquean otras filas válidas, pero una falla inesperada revierte archivo, detalles, errores, historial, auditoría y modificaciones tributarias.

## Limitaciones
- Sin X Factor adicional.
- Sin creación masiva.
- Sin XLSX.
- Sin frontend.
- Sin cambios de estado.
- Sin consulta histórica completa de cargas.
