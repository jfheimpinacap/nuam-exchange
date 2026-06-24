# Contrato API — Copia de Calificaciones Tributarias

## Objetivo

Permitir la copia controlada de una calificación tributaria existente hacia un nuevo registro independiente, con nuevo identificador, creador, timestamps, historial inicial y auditoría propia.

## Endpoint

`POST /api/tax-classifications/{id}/copy`

- No requiere body.
- Usa la política `TaxClassificationWrite`.
- Roles permitidos: `Administrador` y `Analista Tributario`.
- `Supervisor` conserva lectura e historial, pero no puede copiar.

## Campos copiados

La copia replica exclusivamente los campos editables reales:

- `market`
- `instrumentCode`
- `instrumentName`
- `classificationType`
- `description`
- `updatePercentage`
- `appliedFactor`
- `referenceAmount`
- `currency`
- `taxPeriod`
- `validFrom`
- `validTo`

## Campos no copiados

No se copian campos internos ni trazabilidad previa:

- `id`
- `creatorUserId`
- `createdAt`
- `updatedAt`
- `status`
- relaciones de navegación
- historial previo
- auditoría previa
- validaciones tributarias
- archivos, cargas masivas o plantillas relacionadas

## Estado inicial, creador y timestamps

La copia inicia con `VIGENTE`, el mismo estado inicial usado por la creación existente y compatible con el default físico del modelo. No hereda el estado del origen.

`creatorUserId` corresponde al usuario autenticado que ejecuta la copia. `createdAt` y `updatedAt` son timestamps nuevos generados por servidor durante la operación.

## Respuesta exitosa

- `201 Created`
- Cabecera `Location` apuntando a `GET /api/tax-classifications/{nuevoId}`.
- Body con el DTO de detalle de la nueva calificación.

## Errores

- `401 Unauthorized`: JWT ausente o usuario autenticado no identificable.
- `403 Forbidden`: rol sin permiso de escritura, por ejemplo `Supervisor`.
- `404 Not Found`: la calificación origen no existe.
- `503 Service Unavailable`: base de datos o servicio de persistencia no disponible, sin exponer stack trace ni datos internos.

## Historial y auditoría

La nueva calificación recibe un historial inicial independiente con `tipo_cambio = CREACION`, valor permitido por `CK_HistorialCalificacion_TipoCambio` y coherente con el nacimiento de un nuevo registro. El historial previo del origen no se replica ni se modifica.

La auditoría registra la acción `TAX_CLASSIFICATION_COPIED` sobre `CalificacionTributaria`, asociada al id nuevo y al usuario actor.

## Limitaciones

- Sin personalización de copia.
- Sin generación automática de código de instrumento.
- Sin cambio manual de estado.
- Sin validación supervisora.
- Sin cargas masivas X Factor o X Monto.
