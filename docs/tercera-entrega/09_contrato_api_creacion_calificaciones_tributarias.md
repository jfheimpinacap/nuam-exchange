# Contrato API — Creación de Calificaciones Tributarias

## Objetivo

El endpoint `POST /api/tax-classifications` permite crear una calificación tributaria de forma controlada, asociando automáticamente al usuario autenticado como creador y registrando historial y auditoría inicial.

## Autorización

Requiere JWT válido y la política `TaxClassificationWrite`.

Roles permitidos:

- `Administrador`.
- `Analista Tributario`.

Roles no permitidos:

- `Supervisor`, que conserva solo lectura mediante `TaxClassificationRead`.

## Campos aceptados en el request

El cuerpo JSON acepta solo campos editables reales de `TaxClassification`:

- `market` obligatorio, máximo 120 caracteres.
- `instrumentCode` opcional, máximo 80 caracteres.
- `instrumentName` opcional, máximo 180 caracteres.
- `classificationType` obligatorio, máximo 100 caracteres.
- `description` opcional, máximo 500 caracteres.
- `updatePercentage` opcional, decimal, no negativo si viene informado.
- `appliedFactor` opcional, decimal, no negativo si viene informado.
- `referenceAmount` opcional, decimal, no negativo si viene informado.
- `currency` opcional, máximo 10 caracteres; si no se informa se usa `CLP`.
- `taxPeriod` obligatorio, entre 2000 y 2100.
- `validFrom` obligatorio, fecha.
- `validTo` opcional, fecha.

## Campos generados por servidor

El cliente no controla:

- `id`.
- `creatorUserId`.
- `status`.
- `createdAt`.
- `updatedAt`.
- Relaciones de navegación.
- Historial.
- Auditoría.

## Validaciones aplicadas

- Trim seguro de textos.
- Obligatorios según el modelo: `market`, `classificationType`, `currency`, `taxPeriod` y `validFrom`.
- Longitudes máximas definidas por Fluent API.
- `validTo` no puede ser anterior a `validFrom`.
- Checks existentes: período tributario 2000-2100; valores monetarios/factores/porcentajes no negativos.
- No se aplica llave única funcional porque el modelo físico no define índice único de negocio.

## Estado inicial

El estado inicial utilizado es `VIGENTE`, porque la entidad `TaxClassification` y su configuración/migración definen `estado_calificacion` con valor por defecto `VIGENTE` y un CHECK que permite `BORRADOR`, `VIGENTE`, `OBSERVADA`, `REEMPLAZADA` y `ANULADA`.

## Respuesta exitosa

`201 Created` con cuerpo `TaxClassificationDetailDto`, reutilizando el contrato seguro de detalle de consulta.

La cabecera `Location` apunta a:

```http
GET /api/tax-classifications/{id}
```

## Códigos HTTP

- `201 Created`: calificación creada.
- `400 Bad Request`: validación de entrada rechazada.
- `401 Unauthorized`: sin JWT válido o usuario no identificable.
- `403 Forbidden`: JWT válido sin rol permitido, por ejemplo `Supervisor`.
- `503 Service Unavailable`: base de datos o servicio de persistencia no disponible, sin detalles internos.

## Historial inicial

Se registra un `ClassificationHistory` asociado a la calificación y al usuario actor, con `tipo_cambio = CREACION`, compatible con el CHECK existente de `HistorialCalificacion`.

## Auditoría creada

Se registra `Auditoria` con:

- `entidad_afectada = CalificacionTributaria`.
- `registro_afectado_id = id` creado.
- `usuario_id = usuario autenticado`.
- `accion = TAX_CLASSIFICATION_CREATED`.
- Detalle breve y seguro.
- `ip_origen` cuando está disponible.
- `fecha_accion` gestionada desde la operación de servidor.

## Limitaciones

Este contrato no implementa edición, copia, eliminación, validación supervisora, cargas masivas ni frontend.
