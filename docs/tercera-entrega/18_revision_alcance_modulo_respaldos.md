# 18. Revisión y definición de alcance del módulo de Respaldos

## 1. Objetivo de la revisión

Esta revisión define el alcance seguro del módulo de Respaldos en Nuam Exchange a partir de evidencia real del repositorio. El objetivo es separar lo que el modelo persistente permite afirmar de lo que no está implementado ni aprobado operacionalmente.

La decisión actual queda documentada así:

> Nuam Exchange no ejecutará respaldos ni restauraciones desde la aplicación mientras no exista una definición operacional aprobada de infraestructura, permisos, almacenamiento, cifrado, retención, recuperación y autorización.

No se implementaron operaciones de respaldo, restauración, descarga, eliminación, automatización, endpoints, servicios, jobs, pantallas ni registros de prueba.

## 2. Entidad, tabla y campos reales inspeccionados

La entidad real inspeccionada es `BackupRecord`, definida en la capa de dominio y expuesta en el contexto EF Core como `DbSet<BackupRecord> BackupRecords`. Está mapeada por Fluent API a la tabla física `Respaldo`.

Campos reales encontrados:

| Propiedad dominio | Columna SQL Server | Tipo / longitud | Nulabilidad | Default | Observación |
| --- | --- | --- | --- | --- | --- |
| `Id` | `respaldo_id` | `int` identity | Requerido | Identity `1,1` | Clave primaria. |
| `UserId` | `usuario_id` | `int` | Nullable | No | FK opcional a `Usuario`. |
| `BackupType` | `tipo_respaldo` | `nvarchar(50)` | Requerido | No | Catálogo cerrado por CHECK. |
| `BackupPath` | `ruta_respaldo` | `nvarchar(600)` | Requerido | No | Dato textual persistido; no prueba existencia de archivo ni habilita exposición. |
| `BackupStatus` | `estado_respaldo` | `nvarchar(40)` | Requerido | `PROGRAMADO` | Catálogo cerrado por CHECK. |
| `BackupAt` | `fecha_respaldo` | `datetime2(0)` | Requerido | `SYSDATETIME()` | Fecha registrada por base. |
| `Observation` | `observacion` | `nvarchar(700)` | Nullable | No | Texto libre acotado. |
| `User` | N/A | Navegación | Nullable | N/A | Navegación opcional a usuario. |

No existen campos reales para tamaño, hash, nombre de archivo, formato, cifrado, llave, ubicación segura, retención, fecha de expiración, checksum validado, estado de verificación, aprobación, ambiente, base objetivo ni identificador de almacenamiento externo.

## 3. Relaciones, restricciones e índices encontrados

Relación encontrada:

- `Respaldo.usuario_id` referencia opcionalmente `Usuario.usuario_id` mediante `FK_Respaldo_Usuario_usuario_id`.
- La navegación inversa existe en `ApplicationUser.BackupRecords`.
- La relación usa comportamiento `NoAction`, coherente con la convención general del modelo para evitar borrados implícitos de trazabilidad.

Restricciones encontradas:

- `PK_Respaldo` sobre `respaldo_id`.
- `CK_Respaldo_TipoRespaldo`: `tipo_respaldo` debe ser `BASE_DATOS`, `ARCHIVOS` o `COMPLETO`.
- `CK_Respaldo_EstadoRespaldo`: `estado_respaldo` debe ser `PROGRAMADO`, `EJECUTADO`, `FALLIDO` o `RESTAURADO`.
- Longitudes máximas configuradas: `tipo_respaldo` 50, `ruta_respaldo` 600, `estado_respaldo` 40 y `observacion` 700.

Índice encontrado:

- `IX_Respaldo_usuario_id` sobre `usuario_id`.

No se encontraron índices por fecha, estado o tipo de respaldo. Tampoco se encontraron constraints sobre existencia de ruta, tamaño positivo, hash, formato, retención, ambiente, cifrado o aprobación.

## 4. Código, servicios, endpoints, scripts y configuración encontrados

Evidencia encontrada:

- Entidad de dominio `BackupRecord`.
- Configuración Fluent API `BackupRecordConfiguration`.
- `DbSet<BackupRecord>` en `NuamExchangeDbContext`.
- Tabla `Respaldo` en la migración inicial `20260622200947_InitialCreate`.
- Tabla `Respaldo` en el designer de la migración inicial y en el snapshot EF Core.
- Permiso semilla textual `backups.read` en `SecuritySeedService`.
- Documentación previa que menciona respaldos como parte del modelo de 14 tablas.

No se encontraron controllers, endpoints API, casos de uso, interfaces de Application, servicios de Infrastructure, DTOs, jobs, hosted services, Quartz, Hangfire, scripts, pruebas automatizadas ni frontend para respaldos.

No se encontraron comandos `BACKUP DATABASE`, `RESTORE DATABASE`, llamadas a `sqlcmd`, ejecución de comandos del sistema operativo, operaciones de escritura/lectura de archivos, descarga/subida de archivos, escaneo de carpetas, acceso a unidades de red ni integración con almacenamiento cloud relacionados con respaldos.

La configuración de base de datos existente corresponde a la configuración general de EF Core y aplicación; no existe configuración operacional específica de respaldo, restauración, retención, cifrado, rutas, almacenamiento externo o recuperación.

## 5. Clasificación del estado actual

Clasificación seleccionada: **B. Metadatos parciales**.

Justificación:

- Existe una entidad y una tabla para registrar información textual asociada a respaldos.
- El modelo contiene tipo, ruta, estado, fecha, observación y usuario opcional.
- No existe mecanismo seguro de creación, validación, verificación, descarga, restauración, retención ni eliminación.
- No existe evidencia de que `ruta_respaldo` corresponda a un archivo físico existente, accesible, cifrado, íntegro o recuperable.

La categoría **A** no describe completamente el estado porque sí existe un modelo persistente con semántica parcial. La categoría **C** no aplica porque no hay implementación funcional verificable. La categoría **D** no se elige como clasificación principal porque la entidad sí permite inferir metadatos mínimos, aunque insuficientes para operación real.

## 6. Qué puede afirmarse con evidencia

Puede afirmarse que:

- Nuam Exchange contiene la entidad `BackupRecord` y la tabla `Respaldo` en el modelo EF Core.
- La tabla registra metadatos mínimos: usuario opcional, tipo, ruta textual, estado, fecha y observación.
- El modelo restringe tipo y estado mediante CHECK constraints.
- Existe una relación opcional con `Usuario`.
- Existe un permiso semilla `backups.read`, pero no está conectado a una política, endpoint ni operación implementada de respaldos.
- No se modificaron migraciones, snapshot, entidades ni Fluent API durante esta revisión.

## 7. Qué no puede afirmarse

No puede afirmarse que:

- La aplicación ejecute backups o restores.
- La tabla `Respaldo` represente archivos físicos existentes.
- Una ruta persistida sea segura para exponer por API.
- Exista cifrado, gestión de claves o almacenamiento seguro.
- Exista validación de integridad, hash, tamaño o formato.
- Exista retención automática o eliminación segura.
- Exista autorización aprobada para consultar, crear, ejecutar o restaurar respaldos.
- SQL Server tenga permisos para `BACKUP DATABASE` o `RESTORE DATABASE`.
- Un rol como `Administrador` pueda restaurar una base desde la aplicación.
- Plesk, unidades de red, servidores externos o cloud storage formen parte del diseño actual.

## 8. Riesgos de implementar respaldos desde la aplicación sin diseño operacional

Implementar respaldos o restauraciones desde la aplicación sin diseño operacional introduciría riesgos críticos:

- Exposición de rutas internas, nombres de servidor o estructura de almacenamiento.
- Descarga accidental de datos tributarios sensibles.
- Elevación de privilegios si la cuenta técnica requiere permisos de backup o restore.
- Restauraciones destructivas o sobre una base incorrecta.
- Pérdida de trazabilidad si la restauración no preserva auditoría y aprobaciones.
- Retención indefinida o eliminación insegura de datos regulados.
- Archivos sin cifrado o con claves no gestionadas.
- Inconsistencia entre ambientes de desarrollo, pruebas y producción.
- Falsa sensación de recuperabilidad si no existen pruebas de restore.
- Disponibilidad afectada por operaciones pesadas ejecutadas desde una API pública.

## 9. Decisión actual de no implementar backup ni restore

Para Prompt 032 se aprueba únicamente la revisión documental y la definición de alcance. No se implementa ninguna función real de respaldo ni restauración.

Quedan explícitamente prohibidos mientras no exista diseño operacional aprobado:

- `BACKUP DATABASE`.
- `RESTORE DATABASE`.
- SQL dinámico de respaldo o restauración.
- `sqlcmd`.
- Comandos del sistema operativo.
- Escritura, lectura, subida o descarga de archivos.
- Escaneo de carpetas, unidades de red o cloud storage.
- Automatización, hosted services, Quartz, Hangfire o procesos en segundo plano.
- Retención automática, eliminación de respaldos o restauración parcial/total.
- Endpoints API o pantallas frontend de respaldo.

## 10. Alcance futuro posible de consulta de metadatos

Una futura **Fase A — Consulta de metadatos de respaldo** podría evaluarse solo si se aprueba una decisión explícita de autorización y si la semántica de la entidad se considera suficiente para el caso de uso.

Condiciones mínimas:

- Solo lectura.
- Sin ejecutar respaldos.
- Sin ejecutar restauraciones.
- Sin descargar archivos.
- Sin exponer rutas físicas completas, hashes completos, secretos ni contenido de archivos.
- Sin inferir existencia física del archivo a partir de `ruta_respaldo`.
- Con DTO seguro y redacción de campos sensibles.
- Con política y roles definidos antes de implementar.
- Con pruebas que demuestren que la consulta no modifica `Respaldo`, `Usuario`, auditoría ni datos tributarios.

El permiso semilla `backups.read` no basta por sí solo para habilitar esta fase; se requiere una decisión explícita de política y roles.

## 11. Requisitos previos para una operación real

Una futura **Fase B — Operación real de respaldos** queda bloqueada hasta contar con decisión explícita sobre:

- Propietario operativo del proceso.
- Ambiente permitido.
- Base de datos objetivo.
- Cuenta técnica y privilegios mínimos.
- Ubicación de almacenamiento.
- Cifrado.
- Gestión de claves.
- Nombres de archivo.
- Retención.
- Eliminación segura.
- Validación de integridad.
- Monitoreo.
- Registro de auditoría.
- Recuperación.
- Restauración.
- Flujo de aprobación.
- Pruebas de recuperación.
- Manejo de incidentes.
- Separación entre desarrollo, pruebas y producción.

Además, antes de cualquier operación real se requerirían criterios de disponibilidad, ventanas operativas, runbooks, responsabilidades de soporte, evidencia de pruebas de restauración y análisis de cumplimiento sobre datos tributarios.

## 12. Consideraciones de seguridad

No se crearon políticas nuevas ni se modificaron `TaxClassificationRead`, `TaxClassificationWrite`, `TaxClassificationSupervise`, JWT, login, usuarios, roles, permisos ni bootstrap.

Cualquier módulo futuro deberá definir explícitamente:

- Qué rol puede consultar metadatos.
- Qué rol puede solicitar una operación.
- Qué rol puede aprobar una restauración.
- Qué acciones requieren doble aprobación.
- Qué operaciones no deben estar disponibles desde una API pública.
- Qué campos deben redactarse o excluirse del contrato HTTP.
- Cómo se auditan accesos, solicitudes, aprobaciones, fallos y cancelaciones.

No debe asumirse que el rol `Administrador` puede ejecutar o aprobar restauraciones desde la aplicación.

## 13. Consideraciones de retención y recuperación

El modelo actual no define política de retención, fecha de expiración, eliminación segura, ciclos de almacenamiento, copias inmutables, pruebas de recuperación ni RPO/RTO.

Toda decisión futura debe resolver al menos:

- Cuánto tiempo conservar respaldos.
- Quién autoriza eliminación.
- Cómo validar integridad periódicamente.
- Cómo aislar respaldos entre ambientes.
- Cómo documentar pruebas de recuperación.
- Cómo actuar ante incidentes o respaldo corrupto.

## 14. Consideraciones de auditoría

La auditoría existente está modelada en `Auditoria` y ya se usa para operaciones tributarias y seguridad. Sin embargo, el módulo de respaldos no tiene eventos definidos ni operaciones implementadas.

Una fase futura deberá definir eventos auditables para consulta de metadatos, solicitudes, aprobaciones, ejecución externa, fallos, cancelaciones, verificaciones de integridad y restauraciones. También deberá evitar registrar secretos, rutas completas sensibles, cadenas de conexión, hashes completos o contenido de archivos.

## 15. Limitaciones del alcance actual

Esta revisión se limita a inspección estática y documentación. No valida archivos físicos, instancias SQL Server, permisos reales de motor, rutas locales/remotas, cloud storage, Plesk, credenciales, bases `NuamTributariaDB` o `NuamTributariaDB_Dev`, ni mecanismos externos de respaldo.

No se ejecutaron migraciones, actualizaciones de base, comandos SQL de backup/restore, scripts operacionales ni creación de datos de prueba.

## 16. Próximo paso recomendado

Dado que la clasificación actual es **B. Metadatos parciales**, el próximo paso recomendado es **Prompt 033 — Consulta de Metadatos de Respaldos**, condicionado a dos decisiones previas:

1. Confirmar que la semántica de `Respaldo` es suficiente para una consulta de solo lectura.
2. Aprobar explícitamente una política de autorización y roles para consultar esos metadatos.

Si esas decisiones no se aprueban, el módulo debe quedar temporalmente cerrado para implementación funcional y mantenerse solo como modelo persistente documentado.
