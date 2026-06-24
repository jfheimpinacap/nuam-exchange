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
