# Contrato API: validación supervisora de calificaciones tributarias

## Objetivo

Registrar una decisión supervisora sobre una `CalificacionTributaria` existente, usando la tabla física `ValidacionTributaria`, actualizando el estado solo en transiciones soportadas por los CHECK existentes, y dejando trazabilidad en `HistorialCalificacion` y `Auditoria`.

## Endpoint

`POST /api/tax-classifications/{id}/supervisor-validation`

Requiere JWT válido y la política `TaxClassificationSupervise`.

## Body real del request

El contrato usa nombres JSON camelCase explícitos:

```json
{
  "decision": "OBSERVADO",
  "observation": "Texto opcional persistido como observación"
}
```

- `decision` es obligatorio.
- `observation` es opcional y se persiste porque existe como `observacion` en `ValidacionTributaria` y `HistorialCalificacion`.
- No se aceptan campos decorativos ni decisiones de texto libre.

## Roles

Permitidos:

- `Administrador`.
- `Supervisor`.

Denegado:

- `Analista Tributario` recibe `403 Forbidden`.

Sin JWT se responde `401 Unauthorized`.

## Estados y decisiones permitidas

La implementación se limita a valores ya existentes:

- `TaxClassification.Status`: `BORRADOR`, `VIGENTE`, `OBSERVADA`, `REEMPLAZADA`, `ANULADA`.
- `ValidacionTributaria.Result`: `VALIDADO`, `OBSERVADO`, `RECHAZADO`, `APROBADO`.
- `HistorialCalificacion.ChangeType`: `CREACION`, `MODIFICACION`, `ANULACION`, `REEMPLAZO`, `OBSERVACION`, `APROBACION`.

Decisiones aceptadas en este primer alcance:

- `OBSERVADO`.
- `VALIDADO`.
- `APROBADO`.

`RECHAZADO` existe en `ValidacionTributaria`, pero no se expone para calificaciones tributarias porque no existe un estado final compatible `RECHAZADA` en `CalificacionTributaria`.

## Transiciones implementadas

| Estado origen | Decisión | Estado resultante | Historial |
| --- | --- | --- | --- |
| `VIGENTE` | `OBSERVADO` | `OBSERVADA` | `OBSERVACION` |
| `OBSERVADA` | `VALIDADO` | `VIGENTE` | `APROBACION` |
| `OBSERVADA` | `APROBADO` | `VIGENTE` | `APROBACION` |

Cualquier otra combinación con una decisión válida responde `409 Conflict` y no crea registros parciales.

## Respuesta 200 OK

Devuelve el DTO seguro existente `TaxClassificationDetailDto` con la calificación actualizada.

## Errores HTTP

- `400 Bad Request`: body ausente, decisión vacía, decisión no permitida u observación sobre el largo permitido de 700 caracteres.
- `401 Unauthorized`: falta JWT o no puede identificarse el usuario autenticado.
- `403 Forbidden`: rol autenticado sin permiso de supervisión.
- `404 Not Found`: calificación inexistente.
- `409 Conflict`: decisión válida, pero transición no permitida desde el estado actual.
- `503 Service Unavailable`: falla segura de infraestructura o base de datos, sin stack trace ni detalles internos.

## Persistencia y trazabilidad

`ValidacionTributaria` guarda un registro nuevo con `calificacion_id`, `usuario_id`, `resultado`, `observacion` y `fecha_validacion`. No se modifican validaciones anteriores.

`HistorialCalificacion` guarda `calificacion_id`, `usuario_id`, `tipo_cambio`, `campo_modificado = Status`, `valor_anterior`, `valor_nuevo`, `observacion` y `fecha_cambio`.

`Auditoria` registra la entidad `CalificacionTributaria`, el id validado, el actor, la acción exacta `TAX_CLASSIFICATION_VALIDATED`, valores anterior/nuevo de estado, detalle breve, IP de origen cuando esté disponible y fecha de servidor.

## Consistencia transaccional

La operación se ejecuta en una transacción EF Core: lectura, validación de transición, creación de `ValidacionTributaria`, actualización de `Status` y `UpdatedAt`, creación de historial, creación de auditoría y guardado. Ante error o transición inválida no quedan validaciones, historiales, auditorías ni cambios de estado parciales.

## Limitaciones de este primer alcance

No existe endpoint genérico para cambios manuales de estado. No se crean estados nuevos, migraciones, columnas, tablas ni cambios físicos. No se implementa frontend ni flujo de cargas masivas.
