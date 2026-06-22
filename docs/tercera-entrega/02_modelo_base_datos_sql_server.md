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
