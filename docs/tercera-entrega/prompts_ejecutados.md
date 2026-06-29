# Prompts ejecutados

## Prompt 001 — Auditoría Cloud

- Fecha: 2026-06-22.
- Resultado: detenido sin cambios durante auditoría Cloud.
- Observación: no se declara implementación en esta etapa.

## Prompt 002 — Implementación de base técnica

- Fecha: 2026-06-22.
- Resultado: implementación de base técnica inicial.
- Alcance: solución .NET 8, frontend React + TypeScript + Vite, endpoint `/health`, prueba de integración preparada, configuración segura y documentación progresiva.

## Prompt 003 — Corrección de pruebas base y compilación local .NET

- Fecha: 2026-06-22.
- Objetivo: corregir la compilación de la prueba de integración de `GET /health`.
- Causa detectada: tipos de xUnit no resueltos durante compilación local (`IClassFixture<>`, `FactAttribute` y `Fact`) en `HealthEndpointTests.cs`.
- Corrección aplicada: se revisó el proyecto de pruebas, la solución y la clase `Program`; se agregó la directiva explícita `using Xunit;` en la prueba de integración.
- Limitación Cloud: no hay SDK .NET disponible para ejecutar `dotnet restore`, `dotnet build` ni `dotnet test` en Codex Cloud.
- Validación local pendiente posterior al merge: restore, build, test, ejecución local de `/health` y verificación de Swagger.

## Limitaciones de Cloud registradas

- Puede no existir SQL Server local.
- No se tocó Plesk ni producción.
- Las pruebas .NET quedan pendientes si el SDK .NET no está disponible.
- No se deben declarar pruebas exitosas sin ejecución real.

## Pruebas pendientes de validación local

- `dotnet restore`.
- `dotnet build`.
- `dotnet test`.
- Ejecución manual de `/health`.
- Swagger en ambiente Development.
- Frontend local con Vite.

## Prompt 004 — Corrección TypeScript/Vite y advertencia Fast Refresh

- Fecha: 2026-06-22.
- Objetivo: corregir la configuración TypeScript/Vite del frontend y eliminar la advertencia de Fast Refresh en `src/main.tsx`.
- Causa: `moduleResolution: "Node"` queda asociado a resolución `node10`, obsoleta bajo la versión actual de TypeScript y no adecuada como configuración moderna para React + Vite.
- Corrección aplicada: uso de `moduleResolution: "Bundler"` y separación del componente principal hacia `App.tsx`, manteniendo `main.tsx` solo para el montaje de React.
- Limitación Cloud: npm puede no instalar dependencias o no ejecutar scripts por restricción externa de acceso al registro o disponibilidad del entorno.
- Validación local obligatoria posterior al merge: ejecutar `npm install`, `npm run lint`, `npm run build`, `npm run dev` y revisar `http://localhost:5173`.

## Prompt 005 — Corrección de tipado Vite/CSS y exclusión de artefactos TypeScript

- Fecha: 2026-06-22.
- Objetivo: corregir el build TypeScript/Vite del frontend y excluir artefactos `.tsbuildinfo`.
- Causa: TypeScript no reconocía la importación CSS lateral `import './styles.css';` en `src/main.tsx` durante `tsc -b && vite build`.
- Corrección aplicada: se agregó la referencia estándar `vite/client` en `frontend/src/vite-env.d.ts` y se excluyeron los artefactos `*.tsbuildinfo` desde `.gitignore`.
- Limitación Cloud: npm puede no instalar dependencias o no ejecutar scripts por restricciones del entorno, como acceso al registro npm o disponibilidad de dependencias locales.
- Validación local obligatoria posterior al merge: ejecutar `npm run lint`, `npm run build`, `npm run dev` y revisar `http://localhost:5173`.

## Prompt 006 — Modelo de dominio y persistencia EF Core para SQL Server

- Fecha real: 2026-06-22.
- Objetivo: implementar entidades de dominio, `DbContext`, mapeos EF Core, restricciones e índices para SQL Server.
- Alcance realizado: se agregaron 14 entidades persistentes, configuraciones Fluent API, registro condicional de Infrastructure y placeholders seguros de connection string.
- Limitación Cloud: si no existe SDK .NET disponible, no se puede compilar ni generar migraciones en Cloud; las validaciones locales quedan como obligatorias posteriores al merge.
- Validaciones locales obligatorias posteriores al merge: restore, build, test, inicio de API en Development, `/health`, `/swagger` y arranque sin SQL Server configurado.
- Confirmación: no se creó base de datos real, no se ejecutó `database update` y no se generaron migraciones.

## Prompt 007 — Preparación de migraciones locales SQL Server y DbContext de diseño

- Fecha real: 2026-06-22.
- Objetivo: preparar el `DbContext` de diseño y la herramienta local `dotnet-ef` para generar migraciones EF Core de SQL Server posteriormente desde un computador local.
- Alcance realizado: se agregó manifiesto local de herramientas con `dotnet-ef` 8.0.11, paquete de diseño EF Core en la API, factory de diseño `NuamExchangeDbContextFactory`, ejemplo seguro de `appsettings.Development.example.json` y documentación de preparación local.
- Limitación Cloud: no se ejecuta `dotnet-ef`, no se generan migraciones y no se crea `NuamTributariaDB` en Codex Cloud.
- Validaciones locales pendientes: `dotnet restore`, `dotnet build`, `dotnet test`, `dotnet tool restore`, creación local de `appsettings.Development.json`, generación controlada de la migración inicial, revisión de archivos generados y aplicación posterior solo con confirmación explícita.
- Confirmación: no se creó base de datos real, no se ejecutó `database update` y no se generaron migraciones.

## Prompt 008 — Migración inicial EF Core

- Fecha: 2026-06-22.
- Alcance: migración inicial `InitialCreate` generada localmente desde EF Core y aplicada solo a la base local `NuamTributariaDB_Dev`.
- Confirmación: la base original `NuamTributariaDB` se mantuvo como referencia y no fue modificada.
- Limitaciones Cloud: no se asumió acceso a SQL Server desde Codex Cloud.

## Prompt 009 — Autenticación JWT, roles, permisos y bootstrap local seguro

- Fecha: 2026-06-22.
- Alcance: BCrypt, JWT configurable, endpoints `/api/auth/login`, `/api/auth/me`, `/api/auth/permissions`, seed idempotente de roles/permisos y bootstrap Development para primer administrador.
- Limitaciones Cloud: si no existe SDK .NET o SQL Server local, restore/build/test y validaciones con base quedan pendientes para ejecución local posterior al merge.
- Validaciones locales obligatorias posteriores al merge: `dotnet restore`, `dotnet build`, `dotnet test`, configurar JWT local, ejecutar bootstrap local, probar login correcto e incorrecto, `/api/auth/me`, `/api/auth/permissions` y revisar auditoría.
- Confirmación: no se tocó Plesk, SQL Server remoto ni producción.

## Prompt 010 — Corrección global de xUnit en pruebas de autenticación

- Fecha real: 2026-06-22.
- Objetivo: corregir la compilación de las pruebas de autenticación agregadas en Prompt 009.
- Causa: `AuthenticationTests.cs` no resolvía los símbolos de xUnit (`FactAttribute`, `Fact` y `Assert`) durante la compilación local con .NET 8.
- Corrección aplicada: se agregó `GlobalUsings.cs` en el proyecto `NuamExchange.Api.Tests` con `global using Xunit;` para disponibilizar xUnit de forma mantenible a todas las pruebas.
- Paquetes: se mantuvieron las referencias existentes a `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio` y `Microsoft.AspNetCore.Mvc.Testing`; no se agregaron paquetes duplicados.
- Limitación Cloud: no hay SDK .NET disponible para ejecutar `dotnet restore`, `dotnet build` ni `dotnet test` en Codex Cloud.
- Validación local posterior obligatoria: ejecutar restore, build y test completos de la solución después del merge de Prompt 010.

## Prompt 011 — Gestión administrativa segura de usuarios y consulta de roles/permisos

Fecha real de ejecución: 2026-06-23.

Alcance implementado:

- Backend de administración bajo `/api/admin` protegido por `AdministratorOnly`.
- Endpoints creados: `GET /users`, `GET /users/{id}`, `POST /users`, `PUT /users/{id}`, `POST /users/{id}/reset-password`, `GET /roles`, `GET /permissions`.
- DTOs tipados para requests/responses administrativos.
- Política reutilizable de contraseñas aplicada a bootstrap, creación de usuarios y reset de contraseña.
- Auditoría segura para `USER_CREATED`, `USER_UPDATED` y `USER_PASSWORD_RESET`.
- Protecciones para evitar desactivar/degradar al último Administrador activo.

Restricciones respetadas:

- Sin migraciones.
- Sin modificaciones al modelo de entidades ni Fluent API.
- Sin cambios en frontend.
- Sin ejecución de `dotnet ef database update`.
- Sin modificación de bases de datos, Plesk, hosting ni SQL Server remoto.

Limitaciones de Codex Cloud:

- El entorno no dispone de `dotnet`, por lo que restore, build y test quedan como validación local posterior obligatoria.

Validaciones locales obligatorias posteriores al merge:

- `dotnet restore .\NuamExchange.sln`
- `dotnet build .\NuamExchange.sln --no-restore`
- `dotnet test .\NuamExchange.sln --no-build`
- iniciar API con configuración JWT local;
- crear un usuario Analista Tributario;
- consultar listado de usuarios;
- iniciar sesión como Analista;
- comprobar HTTP 403 al acceder a `/api/admin/users`;
- actualizar estado de usuario;
- restablecer contraseña;
- revisar Auditoria.

## Prompt 012 — Gestión controlada de roles y asignación de permisos

- **Fecha real:** 2026-06-23.
- **Alcance:** administración backend de roles personalizados y asignación controlada de permisos existentes.
- **Endpoints creados o extendidos:** `GET /api/admin/roles`, `GET /api/admin/roles/{id}`, `POST /api/admin/roles`, `PUT /api/admin/roles/{id}`, `PUT /api/admin/roles/{id}/permissions` y `GET /api/admin/permissions`.
- **Roles base protegidos:** `Administrador`, `Analista Tributario` y `Supervisor` no pueden modificarse, desactivarse ni recibir cambios de permisos por los nuevos endpoints.
- **Modelo de datos:** sin migraciones, sin nuevas tablas, sin cambios en entidades ni Fluent API.
- **Limitaciones de Codex Cloud:** si `dotnet` no está disponible, restore/build/test quedan como validación local obligatoria posterior al merge.
- **Validaciones locales posteriores al merge:** restaurar, compilar, ejecutar pruebas, levantar API con JWT local, verificar endpoints administrativos y revisar auditoría.

## Prompt 013 — Corrección de binding JSON en creación de roles

- **Fecha real:** 2026-06-23.
- **Objetivo:** corregir y proteger con pruebas el binding JSON de `POST /api/admin/roles` para aceptar el contrato publicado de creación de roles con `description` string opcional o `null`.
- **Contrato HTTP:** creación válida de rol personalizado responde `201 Created`, retorna el DTO seguro del rol creado y documenta respuestas `400`, `401`, `403` y `409` en Swagger.
- **Modelo de datos:** sin migraciones, sin cambios de modelo, sin entidades nuevas y sin cambios en Fluent API.
- **Frontend:** sin cambios.
- **Validación local posterior obligatoria:** restaurar, compilar, ejecutar pruebas, levantar API en `Development`, autenticar como Administrador, crear rol con permisos 7 y 8, consultar detalle del rol y revisar Auditoria para `ROLE_CREATED`.

## Prompt 014 — 2026-06-23 — API de consulta de Calificaciones Tributarias

- Alcance: consulta de solo lectura para Calificaciones Tributarias.
- Endpoints creados: `GET /api/tax-classifications`, `GET /api/tax-classifications/{id}` y `GET /api/tax-classifications/filter-options`.
- Política de acceso: `TaxClassificationRead`, limitada a `Administrador`, `Analista Tributario` y `Supervisor`.
- Sin migraciones.
- Sin cambios de modelo físico ni Fluent API.
- Sin cambios de frontend.
- Limitaciones de Codex Cloud: las validaciones de SDK .NET dependen de disponibilidad del entorno; no se debe declarar éxito si el SDK no está disponible.
- Validaciones locales posteriores obligatorias: `dotnet restore .\NuamExchange.sln`, `dotnet build .\NuamExchange.sln --no-restore`, `dotnet test .\NuamExchange.sln --no-build`, iniciar API en Development y validar respuestas 200, 400, 401, 403, 404 y listado/opciones vacías cuando no existan registros.

## Prompt 015 — Crear Calificaciones Tributarias y registrar historial/auditoría

- **Fecha real:** 2026-06-23.
- **Alcance:** creación controlada de calificaciones tributarias desde API, reutilizando el modelo físico existente.
- **Endpoint creado:** `POST /api/tax-classifications`.
- **Política agregada:** `TaxClassificationWrite`, permitida a `Administrador` y `Analista Tributario`; `Supervisor` queda excluido de escritura.
- **Historial y auditoría:** la creación registra `HistorialCalificacion` con `CREACION` y `Auditoria` con `TAX_CLASSIFICATION_CREATED`.
- **Sin migraciones:** no se creó migración nueva.
- **Sin cambios de modelo:** no se modificaron entidades ni Fluent API.
- **Sin frontend:** no se modificó la interfaz web.
- **Limitaciones de Codex Cloud:** las validaciones contra una base SQL Server real quedan pendientes para ambiente local o de QA con credenciales seguras.
- **Validaciones locales posteriores obligatorias:** restaurar, compilar, ejecutar pruebas, iniciar API en `Development`, autenticar como Administrador y Analista Tributario, crear calificación válida, verificar listado/detalle, confirmar `403` para Supervisor, confirmar `400` para rango de fechas inválido, revisar `HistorialCalificacion` y revisar `Auditoria` para `TAX_CLASSIFICATION_CREATED`.

## Prompt 016 — Corrección de binding JSON en creación de Calificaciones Tributarias

- **Fecha real:** 2026-06-24.
- **Objetivo:** corregir el binding JSON de `POST /api/tax-classifications` para aceptar el contrato publicado de `CreateTaxClassificationRequest`, especialmente `description` como string opcional o `null`.
- **Contrato HTTP:** creación válida de calificación tributaria responde `201 Created`, retorna el detalle seguro e incluye cabecera `Location`; Swagger documenta request y respuestas `201`, `400`, `401`, `403` y `503`.
- **Modelo de datos:** sin migraciones y sin cambios de modelo.
- **Frontend:** sin cambios.
- **Base de datos:** sin modificaciones ni ejecución de `dotnet ef database update`.
- **Validación local posterior obligatoria:** restaurar, compilar, ejecutar pruebas, iniciar API en `Development`, autenticar como Administrador, crear una calificación con el JSON documentado, verificar `201 Created`, `Location`, listado, detalle, `ClassificationHistory` y Auditoria con `TAX_CLASSIFICATION_CREATED`.

## Prompt 017 — Editar Calificaciones Tributarias y consultar historial — 2026-06-24

- **Endpoints creados:** `PUT /api/tax-classifications/{id}` y `GET /api/tax-classifications/{id}/history`.
- **Políticas usadas:** `TaxClassificationWrite` para edición y `TaxClassificationRead` para historial.
- **Modelo:** sin migraciones, sin cambios de entidades, sin cambios de Fluent API y sin cambios de base de datos.
- **Frontend:** sin modificaciones.
- **Historial y auditoría:** la edición crea `ClassificationHistory` con `tipo_cambio = MODIFICACION` y `AuditLog` con `TAX_CLASSIFICATION_UPDATED`.
- **Validaciones locales posteriores obligatorias:** restaurar, compilar, ejecutar pruebas, iniciar API en `Development`, autenticar como Administrador y Analista Tributario, editar una calificación existente, verificar `200 OK`, confirmar campos protegidos, consultar historial, probar `403` como Supervisor y revisar Auditoria.
- **Limitación:** la copia de calificaciones queda pendiente para Prompt 018.

## Prompt 018 — Copiar Calificaciones Tributarias con historial y auditoría

- **Fecha real:** 2026-06-24.
- **Endpoint creado:** `POST /api/tax-classifications/{id}/copy`.
- **Política usada:** `TaxClassificationWrite`, permitida a `Administrador` y `Analista Tributario`.
- **Sin migraciones:** no se crearon migraciones ni cambios de esquema.
- **Sin cambios de modelo:** no se modificaron entidades ni Fluent API.
- **Sin frontend:** no se modificó la aplicación frontend.
- **Historial y auditoría de copia:** la copia registra `ClassificationHistory` con `CREACION` y `AuditLog` con `TAX_CLASSIFICATION_COPIED`.
- **Validaciones locales posteriores obligatorias:** restaurar, compilar, ejecutar pruebas, iniciar API en `Development`, autenticar como Administrador y Analista Tributario, ejecutar `POST /api/tax-classifications/{id}/copy` sin body, verificar `201 Created`, `Location`, id nuevo, campos editables copiados, origen sin cambios, historial `CREACION`, rechazo `403` para Supervisor y Auditoria con `TAX_CLASSIFICATION_COPIED`.

## Prompt 019 - Estados y validación supervisora de Calificaciones Tributarias

- **Fecha real de ejecución:** 2026-06-24.
- **Endpoint creado:** `POST /api/tax-classifications/{id}/supervisor-validation`.
- **Política usada:** `TaxClassificationSupervise`.
- **Roles permitidos:** `Administrador` y `Supervisor`.
- **Rol denegado:** `Analista Tributario`.
- **Estados y transiciones implementados:** `VIGENTE + OBSERVADO -> OBSERVADA`; `OBSERVADA + VALIDADO -> VIGENTE`; `OBSERVADA + APROBADO -> VIGENTE`.
- **Uso de ValidacionTributaria:** se crea un registro por decisión con `calificacion_id`, `usuario_id`, `resultado`, `observacion` y fecha de servidor.
- **Historial y auditoría:** se registra `HistorialCalificacion` con `OBSERVACION` o `APROBACION` y `Auditoria` con acción `TAX_CLASSIFICATION_VALIDATED`.
- **Sin migraciones:** no se generó ni modificó migración.
- **Sin cambios de modelo físico:** no se modificaron entidades, Fluent API, snapshot, tablas, columnas ni constraints.
- **Sin frontend:** no se tocaron pantallas ni assets frontend.
- **Validación local posterior obligatoria:** restaurar, compilar, ejecutar pruebas, iniciar API en `Development`, autenticar con un Administrador real, validar una calificación en estado permitido, revisar detalle, historial, validación y auditoría, y confirmar `409 Conflict` para una transición inválida.

## Prompt 020 — Carga masiva X Factor de Calificaciones Tributarias

- **Fecha real de ejecución:** 2026-06-24.
- **Endpoint creado:** `POST /api/tax-classifications/bulk-loads/x-factor`.
- **Política usada:** `TaxClassificationWrite`.
- **Formato CSV:** UTF-8 con o sin BOM, delimitado por punto y coma, header `market;instrumentCode;taxPeriod;appliedFactor`.
- **Campo actualizado:** solo `AppliedFactor` y `UpdatedAt` en calificaciones existentes con coincidencia única.
- **Entidades de carga utilizadas:** `UploadTemplate`, `UploadFile`, `BulkUploadDetail` y `BulkUploadError` existentes.
- **Historial y auditoría:** filas aplicadas generan `ClassificationHistory` con `MODIFICACION` y `AuditLog` con `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED`.
- **Sin migraciones:** no se agregan migraciones.
- **Sin cambios de modelo físico:** no se modifican entidades, Fluent API, snapshot ni constraints.
- **Sin frontend:** no se implementa UI.
- **Validación local posterior obligatoria:** restaurar, compilar, probar, iniciar API en `Development`, autenticar roles reales disponibles, cargar CSV válido e inválido y confirmar trazabilidad.

## Prompt 021 — Corrección de validación y pruebas de Carga Masiva X Factor

- **Alcance:** corrección acotada sobre Carga Masiva X Factor implementada en Prompt 020.
- **Nulabilidad:** se corrigió la advertencia CS8629 evitando usar `Value` sobre un decimal nullable en la validación de `appliedFactor`.
- **Fixture/pruebas:** se separaron los casos de decimal inválido y duplicidad para que el CSV pruebe identidades inequívocas y la expectativa refleje la prioridad funcional.
- **Regla preservada:** una fila inválida no consume la identidad; solo una identidad procesada correctamente genera `DUPLICATE_ROW` en ocurrencias posteriores.
- **Sin migraciones:** no se agregaron migraciones ni cambios de snapshot.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API ni modelo físico.
- **Sin frontend:** no se modificó interfaz de usuario.
- **Validación local posterior obligatoria:** ejecutar restore, build y test de la solución backend; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas antes de continuar con la prueba manual CSV de Prompt 020.
## Prompt 022 — Corrección de compilación y validación final de pruebas X Factor

- **Fecha real de ejecución:** 2026-06-24.
- **Objetivo:** corregir el error de compilación CS1503 en `TaxClassificationBulkLoadXFactorTests` y validar estáticamente la consistencia del fixture X Factor actualizado.
- **Corrección CS1503:** la assertion de factores ambiguos ahora compara `AppliedFactor` como `decimal?` contra una colección `decimal?[]`, alineada con la nulabilidad real de `TaxClassification.AppliedFactor` y sin usar `dynamic` ni casts inseguros.
- **Revisión de assertion y tipos:** se confirmó que la colección evaluada proviene de `ToListAsync()` sobre EF Core y que cada elemento expone `AppliedFactor` nullable; la expectativa mantiene la comprobación de valores `2m` y `3m` sin debilitarla a solo conteo.
- **Cobertura preservada:** el fixture conserva `INVALID_APPLIED_FACTOR` para una identidad no procesada previamente, `DUPLICATE_ROW` para una identidad ya aplicada correctamente y una fila válida posterior a una fila inválida con la misma identidad.
- **Sin migraciones:** no se generaron ni modificaron migraciones.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API, snapshot ni modelo físico.
- **Sin frontend:** no se modificó la aplicación frontend.
- **Validación local posterior obligatoria:** ejecutar `dotnet restore .\NuamExchange.sln`, `dotnet build .\NuamExchange.sln --no-restore` y `dotnet test .\NuamExchange.sln --no-build` sobre código recompilado; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas antes de continuar con la prueba manual de Carga Masiva X Factor.

## Prompt 023 — Carga Masiva X Monto de Calificaciones Tributarias

- **Fecha real:** 2026-06-24.
- **Endpoint:** `POST /api/tax-classifications/bulk-loads/x-amount`.
- **Política:** `TaxClassificationWrite`.
- **Formato CSV:** UTF-8 con o sin BOM, delimitado por punto y coma, encabezado `market;instrumentCode;taxPeriod;referenceAmount`.
- **Campo actualizado:** solo `ReferenceAmount` / `monto_referencia` y `UpdatedAt` en calificaciones existentes con coincidencia única.
- **Entidades de carga usadas:** `UploadTemplate`, `UploadFile`, `BulkUploadDetail` y `BulkUploadError` existentes con tipo real `X_MONTO`.
- **Historial:** `ClassificationHistory` con `ChangeType = MODIFICACION` y `ModifiedField = ReferenceAmount`.
- **Auditoría:** `AuditLog` con acción `TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED`.
- **Sin migraciones:** no se crearon migraciones nuevas.
- **Sin cambios de modelo:** no se modificaron entidades de dominio, Fluent API ni snapshot.
- **Sin frontend:** no se modificó la aplicación cliente.
- **Validación local posterior obligatoria:** ejecutar restore, build, test, levantar API en Development y validar manualmente una fila válida más una inexistente sobre base local.

## Prompt 024 — Corrección de fakes de pruebas tras Carga Masiva X Monto

- **Fecha real:** 2026-06-24.
- **Corrección CS0535:** se actualizaron los fakes manuales de pruebas que implementan `ITaxClassificationCommandService` y habían quedado incompletos tras incorporar `BulkLoadXAmountAsync`.
- **Fakes de pruebas actualizados:** copia, binding JSON de endpoint y validación supervisora implementan explícitamente el nuevo miembro con `NotSupportedException` porque no participan en pruebas de Carga Masiva X Monto.
- **Cobertura preservada:** se mantuvieron las pruebas previas de consulta, creación, edición/historial, copia, validación supervisora, binding JSON, Carga Masiva X Factor y Carga Masiva X Monto sin debilitar assertions ni eliminar casos.
- **Sin migraciones:** no se crearon ni modificaron migraciones.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API, snapshot ni modelo físico.
- **Sin frontend:** no se modificó la aplicación cliente.
- **Validación local posterior obligatoria:** ejecutar restore, build y test de la solución backend recompilada; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas antes de continuar con la validación manual de Carga Masiva X Monto.

## Prompt 025 — Consulta de Cargas Masivas, Detalles y Errores

- **Fecha real:** 2026-06-24.
- **Endpoints creados:** `GET /api/bulk-loads`, `GET /api/bulk-loads/{id}`, `GET /api/bulk-loads/{id}/details`, `GET /api/bulk-loads/{id}/errors`.
- **Política aplicada:** `TaxClassificationRead` para `Administrador`, `Analista Tributario` y `Supervisor`.
- **Alcance:** consultas de solo lectura sobre `UploadFile`, `UploadTemplate`, `BulkUploadDetail` y `BulkUploadError`.
- **Paginación:** `page` y `pageSize` con límite máximo seguro de 100.
- **Filtros:** tipo y estado de carga reales; estado, campo, calificación y fila en detalles; severidad, columna y fila en errores.
- **Ordenamiento:** lista blanca segura para historial de cargas; detalles y errores con orden fijo por fila.
- **Seguridad:** se excluyen rutas físicas, hashes, contenido bruto completo de archivos, stack traces, connection strings, claims JWT y credenciales.
- **Base de datos:** sin migraciones, sin cambios físicos, sin nuevas tablas, columnas, índices ni relaciones.
- **Frontend:** sin cambios.
- **Validación local posterior obligatoria:** restaurar, compilar, probar, iniciar API en Development, autenticarse con Administrador real y verificar listado, resumen, detalles, errores, aislamiento entre cargas, `404` para id inexistente y ausencia de modificaciones en datos tributarios o registros de carga.

## Prompt 026 — Corrección de consultas y pruebas de Cargas Masivas

- **Fecha real:** 2026-06-24.
- **Alcance:** corrección mínima de fixtures y assertions de pruebas para los endpoints de consulta de cargas masivas implementados en Prompt 025.
- **Causa real detectada:** el fixture de integración generaba el nombre de base EF Core InMemory dentro de `AddDbContext`, permitiendo que el seed y las consultas usaran bases distintas; por eso una carga sembrada podía no aparecer en listado, resumen, detalles ni errores.
- **Corrección aplicada:** se captura un único nombre InMemory por `WebApplicationFactory` y se reutiliza para sembrado y consultas; los IDs sembrados vuelven a coincidir con los endpoints `/api/bulk-loads/{id}`.
- **xUnit2012:** se reemplazó `Assert.False(...Any(...))` por `Assert.DoesNotContain(...)` para comprobar ausencia de entidades modificadas en el `ChangeTracker`.
- **Endpoints preservados:** `GET /api/bulk-loads`, `GET /api/bulk-loads/{id}`, `GET /api/bulk-loads/{id}/details` y `GET /api/bulk-loads/{id}/errors` siguen siendo de solo lectura y mantienen `404` solo para cargas inexistentes.
- **Colecciones:** details y errors devuelven `PagedResult` con `items` no nulo, incluyendo colecciones vacías cuando la carga existe sin hijos.
- **Seguridad:** no se exponen `FilePath`, `FileHash` ni rutas físicas.
- **Sin migraciones:** no se crearon ni modificaron migraciones.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API, snapshot ni modelo físico.
- **Sin frontend:** no se modificó la aplicación cliente.
- **Validación local posterior obligatoria:** ejecutar `dotnet restore ./backend-dotnet/NuamExchange.sln`, `dotnet build ./backend-dotnet/NuamExchange.sln --no-restore` y `dotnet test ./backend-dotnet/NuamExchange.sln --no-build`; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas antes de continuar con validación manual de consultas de cargas masivas.

## Prompt 027 — Reporte Tributario de Calificaciones y Exportación CSV (2026-06-24)

- **Endpoints:** `GET /api/tax-reports/tax-classifications` y `GET /api/tax-reports/tax-classifications/export`.
- **Política:** `TaxClassificationRead` para `Administrador`, `Analista Tributario` y `Supervisor`.
- **Filtros:** market, instrumentCode, taxPeriod, status, classificationType, currency, sortBy y sortDirection; JSON agrega page/pageSize.
- **Resumen:** total filtrado, conteos por estado/tipo, conteos con factor/monto y suma de ReferenceAmount agrupada por moneda.
- **CSV:** UTF-8 BOM, delimitador `;`, encabezado explícito, escape RFC compatible y neutralización de Formula Injection con apóstrofo.
- **Límite:** 10000 filas; sobre el límite responde 400 sin exportación parcial.
- **ReporteTributario:** inspeccionado y no usado para persistir exportaciones por falta de semántica previa compatible demostrada.
- **Modelo:** sin migraciones, sin cambios físicos, sin cambios de entidades, Fluent API o snapshot.
- **Frontend:** sin cambios.
- **Validación local posterior obligatoria:** `dotnet restore`, `dotnet build --no-restore` y `dotnet test --no-build` sobre `backend-dotnet/NuamExchange.sln`.

## Prompt 028 — Corrección de fixture y pruebas de Reporte Tributario CSV (2026-06-24)

- **Alcance:** corrección mínima de pruebas de Reporte Tributario JSON/CSV tras el fallo `Expected: 3 / Actual: 0` en `TaxReportQueryTests.JsonEndpoint_ReturnsSafeContractAndCsvHeaders`.
- **Causa real:** el fixture de integración de `TaxReportQueryTests` generaba el nombre EF Core InMemory dentro del callback de `AddDbContext`; el seed y la API podían resolver bases distintas aunque el seed hiciera `SaveChanges`.
- **Corrección aplicada:** se captura un único nombre InMemory por `WebApplicationFactory` y se reutiliza para seed, cliente HTTP y contextos de verificación, siguiendo el patrón corregido previamente en `BulkLoadQueryTests`.
- **Cobertura preservada:** se mantuvo la expectativa válida de tres registros `BOLSA`; no se cambió a cero ni se debilitaron assertions.
- **JSON preservado:** `GET /api/tax-reports/tax-classifications` conserva `200 OK`, `items` no nulo, `summary` no nulo, paginación, `totalCount`, `totalPages`, filtros y contrato seguro.
- **CSV preservado:** `GET /api/tax-reports/tax-classifications/export` conserva UTF-8 con BOM, delimitador `;`, encabezado, escape CSV, neutralización de Formula Injection, exclusión de campos internos y mismo conjunto filtrado que JSON.
- **Solo lectura:** las pruebas verifican que JSON y CSV no modifican calificaciones ni crean cargas, detalles, errores, historial, auditoría ni `TaxReport`/ReporteTributario.
- **Sin migraciones:** no se crearon ni modificaron migraciones.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API, snapshot ni modelo físico.
- **Sin frontend:** no se modificó la aplicación cliente.
- **Validación local posterior obligatoria:** ejecutar `dotnet restore ./backend-dotnet/NuamExchange.sln`, `dotnet build ./backend-dotnet/NuamExchange.sln --no-restore` y `dotnet test ./backend-dotnet/NuamExchange.sln --no-build`; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas antes de continuar con validación manual del Reporte Tributario y CSV.

## Prompt 029 — Consulta segura de auditoría tributaria (2026-06-24)

- **Endpoints:** `GET /api/tax-audits` y `GET /api/tax-audits/{id}`.
- **Política:** `TaxClassificationRead` para `Administrador`, `Analista Tributario` y `Supervisor`.
- **Regla tributaria:** `AffectedEntity = CalificacionTributaria`, `AffectedRecordId` no nulo y acción en lista cerrada: `TAX_CLASSIFICATION_CREATED`, `TAX_CLASSIFICATION_UPDATED`, `TAX_CLASSIFICATION_COPIED`, `TAX_CLASSIFICATION_VALIDATED`, `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED`, `TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED`.
- **Filtros y paginación:** `page`, `pageSize` máximo 100, `taxClassificationId`, `action`, `dateFrom`, `dateTo`, `sortBy` con lista blanca y `sortDirection asc|desc`.
- **Campos seguros:** `id`, `action`, `taxClassificationId`, `actorUserId`, `occurredAt`, y en detalle `detail`, `previousValue`, `newValue` bajo regla tributaria.
- **Exclusión:** auditoría no tributaria no se lista y por id responde `404`; no se expone IP, claims, rutas, hashes, emails ni credenciales.
- **Sin migraciones:** no hubo migraciones, cambios de modelo físico, frontend ni snapshot.
- **Validación local posterior obligatoria:** restaurar, compilar, ejecutar pruebas, iniciar API, autenticar roles permitidos, consultar listado/detalle, validar filtros y confirmar que no cambian calificaciones, cargas ni auditoría.
## Prompt 030 — Corrección de referencia PagedResult en Auditoría Tributaria (2026-06-24)

- **Corrección CS0246:** se corrigió la resolución de `PagedResult<>` usada por `ITaxAuditQueryService` en los DTOs de Auditoría Tributaria.
- **PagedResult canónico:** se reutiliza el `PagedResult<T>` existente en `NuamExchange.Application.TaxClassifications`, sin crear tipos duplicados ni mover clases entre capas.
- **Alcance funcional:** no se modificaron filtros, reglas tributarias, servicios, controladores, autorización, DTOs seguros ni endpoints públicos de auditoría.
- **Sin migraciones:** no se crearon ni modificaron migraciones.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API, snapshot ni modelo físico.
- **Sin frontend:** no se modificó la aplicación cliente.
- **Validación local posterior obligatoria:** ejecutar `dotnet restore ./backend-dotnet/NuamExchange.sln`, `dotnet build ./backend-dotnet/NuamExchange.sln --no-restore` y `dotnet test ./backend-dotnet/NuamExchange.sln --no-build`; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas antes de continuar con la validación manual de Auditoría Tributaria.

## Prompt 031 — Corrección de traducción EF Core en Auditoría Tributaria (2026-06-29)

- **Corrección EF Core:** se reemplazó el uso de `IReadOnlySet<string>.Contains` dentro del `IQueryable` de Auditoría Tributaria por un arreglo estático traducible por EF Core SQL Server.
- **Causa del HTTP 503:** la consulta de listado/detalle podía fallar por traducción SQL al evaluar la lista cerrada de acciones mediante `TaxAuditRules.AllowedActions.Contains(x.Action)`; el controlador convertía esa excepción en respuesta segura `503`.
- **Fuente de verdad única:** `TaxAuditRules.AllowedActionValues` contiene las seis acciones tributarias y `AllowedActions` se deriva de ese arreglo para validaciones en memoria.
- **Mensaje 503 y logging:** se preserva el mensaje público seguro en español correcto y se agrega registro interno de la excepción sin exponer detalles al cliente.
- **Sin migraciones:** no se crearon ni modificaron migraciones.
- **Sin cambios de modelo:** no se modificaron entidades, Fluent API, snapshot ni modelo físico.
- **Sin frontend:** no se modificó la aplicación cliente.
- **Validación local posterior obligatoria:** ejecutar `dotnet restore ./backend-dotnet/NuamExchange.sln`, `dotnet build ./backend-dotnet/NuamExchange.sln --no-restore` y `dotnet test ./backend-dotnet/NuamExchange.sln --no-build`; confirmar 0 advertencias, 0 errores y todas las pruebas aprobadas, y luego validar manualmente `GET /api/tax-audits` y detalle con API en Development.
