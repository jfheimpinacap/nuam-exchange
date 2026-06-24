# 02. Modelo de base de datos SQL Server

Prompt 006 implementa el modelo físico como configuraciones EF Core para SQL Server, sin crear aún la base `NuamTributariaDB` ni conectarse a una instancia real. Las migraciones se generarán posteriormente desde este modelo validado.

## Entidades, propósitos y claves

1. `Role` / `Rol`: catálogo de roles; PK `rol_id`; nombre único.
2. `Permission` / `Permiso`: catálogo de permisos; PK `permiso_id`; código único.
3. `RolePermission` / `RolPermiso`: relación rol-permiso; PK `rol_permiso_id`; FK a `Rol` y `Permiso`; combinación única.
4. `ApplicationUser` / `Usuario`: usuarios internos; PK `usuario_id`; FK `rol_id`; email único.
5. `TaxClassification` / `CalificacionTributaria`: calificaciones tributarias; PK `calificacion_id`; FK `usuario_creador_id` a `Usuario`.
6. `ClassificationHistory` / `HistorialCalificacion`: trazabilidad de cambios; PK `historial_id`; FK a `CalificacionTributaria` y `Usuario`.
7. `UploadTemplate` / `PlantillaCarga`: plantillas de carga; PK `plantilla_id`; combinación única tipo-versión.
8. `UploadFile` / `ArchivoCarga`: archivos cargados; PK `archivo_carga_id`; FK a `Usuario` y `PlantillaCarga`.
9. `BulkUploadDetail` / `DetalleCargaMasiva`: filas procesadas; PK `detalle_carga_id`; FK a `ArchivoCarga` y opcional a `CalificacionTributaria`.
10. `BulkUploadError` / `ErrorCargaMasiva`: errores de carga; PK `error_carga_id`; FK a `ArchivoCarga`.
11. `TaxValidation` / `ValidacionTributaria`: validaciones; PK `validacion_id`; FK requerida a `Usuario` y referencia opcional a calificación o archivo, con regla que exige al menos una.
12. `TaxReport` / `ReporteTributario`: reportes generados; PK `reporte_id`; FK a `Usuario`.
13. `AuditLog` / `Auditoria`: auditoría modelada; PK `auditoria_id`; FK opcional a `Usuario`.
14. `BackupRecord` / `Respaldo`: respaldos planificados o ejecutados; PK `respaldo_id`; FK opcional a `Usuario`.

## Reglas de integridad

Se configuraron restricciones para periodos tributarios entre 2000 y 2100, montos/factores no negativos, vigencias coherentes, catálogos cerrados para estados, resultados, severidades, formatos y tipos de carga/respaldo, contadores no negativos, tamaño de archivo positivo cuando exista y obligatoriedad de al menos una referencia en validaciones tributarias.

## Índices principales

Incluyen índices únicos para rol, permiso, email y plantilla por tipo-versión; índices por rol de usuario; índices por periodo/estado, mercado/tipo y vigencias de calificaciones; índices descendentes por fecha para historial, cargas, validaciones, reportes y auditoría; además de índices por archivo/fila y calificación en detalles de carga.

## Eliminaciones en cascada

No se configuraron eliminaciones en cascada. Todas las relaciones usan `NoAction` para proteger historia, auditoría, validaciones y trazabilidad tributaria, evitando borrar registros dependientes de forma implícita.

## Preparación para migraciones EF Core

Prompt 007 deja el modelo EF Core preparado para generar migraciones de forma local con la herramienta versionada `dotnet-ef` 8.0.11. La generación se realizará posteriormente desde el computador de desarrollo, una vez configurada la cadena local segura.

La base `NuamTributariaDB` aún no existe y no fue creada durante esta preparación. La cadena de conexión local no se versiona: cada persona debe crear su propio `appsettings.Development.json` ignorado por Git a partir del ejemplo seguro disponible en la API.

## Actualización Prompt 009: autenticación sobre modelo existente

- La migración `InitialCreate` fue generada desde EF Core y versionada como punto de partida del modelo relacional.
- La base local de desarrollo `NuamTributariaDB_Dev` fue creada con las 14 tablas del modelo oficial de la entrega.
- La base original `NuamTributariaDB` se conserva como referencia y no fue modificada.
- Las tablas existentes de roles, permisos y auditoría se utilizarán para la autenticación, autorización y trazabilidad de accesos.
- No se modificó el modelo lógico, no se agregaron tablas nuevas y no se generaron migraciones adicionales para esta fase.

## Contrato de consulta de CalificacionTributaria — Prompt 014

La API de consulta `/api/tax-classifications` se construyó sobre la entidad oficial `TaxClassification`, mapeada a `CalificacionTributaria`. El contrato expone únicamente campos existentes del modelo físico: `Id`, `CreatorUserId`, `Market`, `InstrumentCode`, `InstrumentName`, `ClassificationType`, `Description`, `UpdatePercentage`, `AppliedFactor`, `ReferenceAmount`, `Currency`, `TaxPeriod`, `ValidFrom`, `ValidTo`, `Status`, `CreatedAt` y `UpdatedAt`.

Los filtros disponibles derivan del modelo oficial existente: `market` usa `Market`, `exercise` usa `TaxPeriod`, `status` usa `Status` y `search` consulta campos de texto reales (`InstrumentCode`, `InstrumentName`, `Description`, `ClassificationType`). El filtro funcional `origin` de mockups no se expone porque no existe una columna o propiedad equivalente en `TaxClassification`.

No se modificó el modelo físico, no se agregaron columnas, no se cambiaron tipos, no se alteró Fluent API y no se creó migración. Las respuestas y opciones de filtro derivan exclusivamente del modelo oficial existente y de valores persistidos en la base de datos.

## Relación CalificacionTributaria - ValidacionTributaria para validación supervisora

`ValidacionTributaria` contiene la PK `validacion_id` y el campo nullable `calificacion_id`, que referencia a `CalificacionTributaria.calificacion_id`. La misma tabla también contiene `archivo_carga_id`; el CHECK `CK_ValidacionTributaria_Referencia` exige que al menos una de esas referencias exista. Para calificaciones tributarias, la validación supervisora persiste `calificacion_id` y no requiere `archivo_carga_id`.

La cardinalidad real es una calificación con cero o muchas validaciones (`TaxClassification.TaxValidations`), sin restricción física de una única validación por calificación. Cada validación se asocia además a `Usuario` mediante `usuario_id`, guarda `resultado`, `observacion` y `fecha_validacion`, y opera como trazabilidad funcional sin modificar el modelo físico ni crear migraciones nuevas.

## Trazabilidad de Carga Masiva X Factor

La Carga Masiva X Factor reutiliza el modelo físico existente sin migraciones ni cambios de columnas. `UploadFile` / `ArchivoCarga` representa el archivo lógico recibido y mantiene la FK obligatoria a `UploadTemplate` / `PlantillaCarga`. Para X Factor se usa `tipo_carga = X_FACTOR`, `extension = CSV` y estados existentes de `ArchivoCarga`.

`BulkUploadDetail` / `DetalleCargaMasiva` depende de `ArchivoCarga` mediante `archivo_carga_id`; cada fila del CSV estructuralmente válido genera un detalle con `numero_fila`, `campo_afectado = AppliedFactor`, `valor_factor`, `valor_texto_original` acotado y estado real permitido (`APLICADA` o `CON_ERROR`). La relación opcional con `TaxClassification` / `CalificacionTributaria` se informa solo cuando la fila llega a una coincidencia única y se actualiza la calificación.

`BulkUploadError` / `ErrorCargaMasiva` también depende de `ArchivoCarga` mediante `archivo_carga_id`; las filas rechazadas registran `numero_fila`, columna cuando aplica, descripción segura y severidad `ERROR`. El modelo existente no contiene FK directa de error a detalle, por lo que la correlación comprobable se realiza por `archivo_carga_id` y `numero_fila`.

Estas entidades permiten demostrar qué archivo fue recibido, qué filas fueron aplicadas o rechazadas y qué errores ocurrieron, sin afirmar ni requerir cambios físicos de base de datos.

## Soporte de Carga Masiva X Monto sin cambios físicos

La Carga Masiva X Monto reutiliza el modelo físico existente sin migraciones ni cambios de columnas. `UploadTemplate` / `PlantillaCarga` y `UploadFile` / `ArchivoCarga` ya admiten `tipo_carga = X_MONTO` mediante los CHECK existentes, por lo que la implementación registra una plantilla lógica CSV versión `1.0` y archivos con `extension = CSV`.

`BulkUploadDetail` / `DetalleCargaMasiva` soporta la carga por medio de `campo_afectado = ReferenceAmount`, `valor_monto` (`decimal(18,4)`), `valor_texto_original`, `numero_fila` y estados existentes `APLICADA` o `CON_ERROR`. La FK opcional a `CalificacionTributaria` se informa solo en filas con coincidencia única aplicada.

`BulkUploadError` / `ErrorCargaMasiva` registra filas rechazadas por archivo, número de fila, columna cuando corresponde (`referenceAmount`) y severidad `ERROR`. La calificación tributaria usa el campo físico `monto_referencia` (`decimal(18,4)`, nullable) con CHECK no negativo, y la carga solo modifica ese campo y `actualizado_en` cuando la fila es válida.

## Consulta de trazabilidad de cargas masivas

La consulta de cargas masivas se apoya en el modelo físico existente sin introducir cambios de esquema. `UploadFile` / `ArchivoCarga` representa el encabezado funcional de una carga, referencia una `UploadTemplate` / `PlantillaCarga` mediante `plantilla_id` y concentra tipo, archivo original, extensión, estado, contadores y fecha de carga.

`BulkUploadDetail` / `DetalleCargaMasiva` se relaciona con `UploadFile` por `archivo_carga_id` y registra cada fila procesada con `numero_fila`, `campo_afectado`, valor de factor o monto según corresponda, estado de fila y, cuando existe coincidencia, `calificacion_id` hacia `TaxClassification` / `CalificacionTributaria`.

`BulkUploadError` / `ErrorCargaMasiva` se relaciona con `UploadFile` por `archivo_carga_id` y conserva errores seguros por fila, columna, descripción y severidad. No existe relación física directa entre error y detalle; la correlación disponible se realiza por carga y número de fila.

Los endpoints de consulta usan proyecciones `AsNoTracking`, filtran siempre por `archivo_carga_id` para detalles y errores, y no actualizan encabezados, detalles, errores ni calificaciones tributarias. Esta sección describe uso de trazabilidad existente y no representa migraciones, columnas, índices, constraints ni relaciones nuevas.

## Prompt 027 — Reporte tributario desde Calificaciones Tributarias

`CalificacionTributaria` es la fuente directa del reporte tributario de consulta y exportación CSV. El reporte usa únicamente campos reales de la entidad: mercado, código y nombre de instrumento, tipo de calificación, período tributario, estado, factor aplicado, monto de referencia, moneda, vigencias y fecha de actualización.

Se inspeccionó `ReporteTributario`: contiene `reporte_id`, `usuario_id`, `tipo_reporte`, `filtros_aplicados`, `formato`, `ruta_reporte` y `generado_en`, con formato restringido a `PDF`, `XLSX` o `CSV` y FK a `Usuario`. No se fuerza su uso porque no existe una implementación previa compatible que demuestre semántica de historial para esta exportación en tiempo real.

No se realizaron migraciones ni cambios físicos: sin nuevas tablas, columnas, índices, relaciones, constraints, cambios de entidad, Fluent API o snapshot.

## Prompt 029 — Consulta segura de Auditoria tributaria

Se inspeccionó la entidad real `AuditLog` mapeada a `Auditoria`: `auditoria_id`, `usuario_id`, `entidad_afectada`, `registro_afectado_id`, `accion`, `detalle`, `valor_anterior`, `valor_nuevo`, `ip_origen` y `fecha_accion`. `Auditoria.usuario_id` mantiene relación opcional con `Usuario`; no existe relación física directa con `CalificacionTributaria`, por lo que el alcance seguro de consulta se define por convención estricta existente: `entidad_afectada = CalificacionTributaria`, `registro_afectado_id` no nulo y acción en lista cerrada tributaria.

Las acciones tributarias incluidas son `TAX_CLASSIFICATION_CREATED`, `TAX_CLASSIFICATION_UPDATED`, `TAX_CLASSIFICATION_COPIED`, `TAX_CLASSIFICATION_VALIDATED`, `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED` y `TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED`. No se realizaron migraciones ni cambios físicos de base de datos, entidades, Fluent API ni snapshot.
