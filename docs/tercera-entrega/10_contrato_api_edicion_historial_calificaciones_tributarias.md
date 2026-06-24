# Contrato API — Edición e historial de Calificaciones Tributarias

## Objetivo

Implementar la actualización controlada de una `CalificacionTributaria` existente y la consulta segura de su historial, sin cambios al modelo físico, sin migraciones y sin frontend.

## Endpoint PUT

`PUT /api/tax-classifications/{id}`

- Política: `TaxClassificationWrite`.
- Roles permitidos: `Administrador` y `Analista Tributario`.
- `Supervisor` conserva solo lectura y recibe `403 Forbidden` al editar.
- Retorna `200 OK` con el DTO seguro de detalle cuando la actualización es válida.

### Campos editables

El contrato `UpdateTaxClassificationRequest` acepta exclusivamente los campos reales editables:

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

### Campos bloqueados y controlados por servidor

No se aceptan ni se modifican desde el body:

- `id`
- `creatorUserId`
- `createdAt`
- `updatedAt`
- `status`
- relaciones de navegación
- historial
- auditoría
- archivos
- validaciones

La actualización mantiene `CreatorUserId`, `CreatedAt`, `Status`, identidad del registro y relaciones existentes. `UpdatedAt` se recalcula en servidor.

## Validaciones aplicadas

Las validaciones reutilizan las reglas de creación y las restricciones reales del modelo:

- trim seguro de textos;
- `market` requerido, máximo 120 caracteres;
- `classificationType` requerido, máximo 100 caracteres;
- `instrumentCode` máximo 80 caracteres;
- `instrumentName` máximo 180 caracteres;
- `description` opcional, máximo 500 caracteres;
- `currency` usa `CLP` por defecto si viene vacío y máximo 10 caracteres;
- `taxPeriod` entre 2000 y 2100;
- `validFrom` obligatorio;
- `validTo` no puede ser anterior a `validFrom`;
- `updatePercentage`, `appliedFactor` y `referenceAmount` no pueden ser negativos cuando se informan.

Los mensajes de validación se responden en español y no exponen detalles internos.

## Respuesta 200

La respuesta `200 OK` usa el DTO seguro de detalle existente, incluyendo identificadores y fechas de servidor para que el cliente confirme que `Status`, `CreatorUserId` y `CreatedAt` permanecen sin cambios y que `UpdatedAt` cambió.

## Errores

- `400 Bad Request`: body inválido o validación funcional fallida.
- `401 Unauthorized`: falta JWT o no se puede identificar al usuario autenticado.
- `403 Forbidden`: JWT válido sin política requerida, por ejemplo `Supervisor` al editar.
- `404 Not Found`: la calificación no existe.
- `503 Service Unavailable`: base de datos no disponible o servicio no registrado, con mensaje seguro.

## Endpoint GET history

`GET /api/tax-classifications/{id}/history`

- Política: `TaxClassificationRead`.
- Roles permitidos: `Administrador`, `Analista Tributario` y `Supervisor`.
- Usa consulta de solo lectura (`AsNoTracking`).
- Retorna `200 OK` con arreglo de historial seguro.
- Si la calificación existe y no tiene historial, retorna arreglo vacío.
- Si la calificación no existe, retorna `404 Not Found`.

El historial se ordena de forma determinista por `ChangedAt` descendente y luego `Id` descendente.

## Historial de modificación

Cada actualización válida registra un nuevo `ClassificationHistory` asociado a la calificación y al usuario actor.

La acción real utilizada es `MODIFICACION`. El fundamento es el CHECK físico `CK_HistorialCalificacion_TipoCambio`, que permite únicamente `CREACION`, `MODIFICACION`, `ANULACION`, `REEMPLAZO`, `OBSERVACION` y `APROBACION`; por lo tanto, `MODIFICACION` es el valor compatible para edición controlada.

Se usa `ModifiedField = CamposEditables` y una observación breve y segura, sin tokens, contraseñas, hashes ni secretos.

## Auditoría

Cada actualización válida registra `AuditLog` sobre la entidad `CalificacionTributaria` con:

- `accion = TAX_CLASSIFICATION_UPDATED`;
- `registro_afectado_id = id` actualizado;
- `usuario_id` del actor autenticado;
- detalle breve y seguro;
- `ip_origen` desde el mecanismo HTTP existente cuando está disponible;
- fecha generada en servidor.

## Limitaciones

Este contrato no implementa copia, eliminación, cambio manual de estado, aprobación supervisora, validación supervisora, cargas masivas X Factor/X Monto ni frontend.
