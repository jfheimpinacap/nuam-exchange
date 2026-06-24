# 05. Pruebas y evidencias

Prompt 006 agrega entidades de dominio, `NuamExchangeDbContext`, configuraciones Fluent API y proveedor SQL Server sin conectar una base real.

## Validaciones pendientes posteriores al merge

- `dotnet restore .\NuamExchange.sln`.
- `dotnet build .\NuamExchange.sln --no-restore`.
- `dotnet test .\NuamExchange.sln --no-build`.
- Inicio de API en Development sin connection string.
- Verificación de `/health`.
- Verificación de `/swagger` en Development.
- Futura migración local controlada para `NuamTributariaDB`.

No se generaron migraciones ni se ejecutó actualización de base de datos en Prompt 006.

## Validaciones futuras para migraciones locales

Prompt 007 agrega la preparación para migraciones locales, pero no genera migraciones ni crea bases de datos en Cloud. Las validaciones futuras obligatorias son:

- Ejecutar `dotnet tool restore` desde `backend-dotnet` para restaurar la herramienta local `dotnet-ef`.
- Crear localmente el archivo ignorado `appsettings.Development.json` con una cadena SQL Server válida para `NuamTributariaDB`.
- Generar de forma controlada la migración inicial con `dotnet-ef`.
- Revisar los archivos generados antes de aplicar cualquier cambio en SQL Server.
- Crear localmente la base `NuamTributariaDB` solo cuando corresponda.
- Verificar tablas, claves, restricciones e índices después de aplicar la migración en el entorno local autorizado.

## Actualización Prompt 009: pruebas de seguridad

### Pruebas unitarias agregadas

- BCrypt: generación de hash, diferencia contra password original, verificación correcta y rechazo de password incorrecto.
- JWT: creación de token no vacío, claims `sub`, `email` y rol, expiración futura y ausencia de password hash.

### Validaciones manuales futuras posteriores al merge

- Bootstrap de administrador local en Development.
- Login correcto.
- Login incorrecto con respuesta genérica.
- Expiración de token.
- Acceso autenticado a `/api/auth/me`.
- Acceso autenticado a `/api/auth/permissions`.
- Registro de auditoría para login exitoso, login fallido y bootstrap.

## Actualización Prompt 010: corrección global de xUnit en pruebas de autenticación

Después del merge de Prompt 009, la validación local inicial con .NET 8 falló al compilar el proyecto de pruebas porque `AuthenticationTests.cs` no resolvía los símbolos de xUnit (`FactAttribute`, `Fact` y aserciones como `Assert`). El resultado posterior de `dotnet test --no-build` no se considera evidencia válida para estas pruebas, porque pudo usar un DLL generado antes del fallo de compilación.

La corrección de Prompt 010 agrega un `GlobalUsings.cs` en el proyecto `NuamExchange.Api.Tests` con `global using Xunit;`, dejando disponible xUnit para todas las clases de prueba del proyecto, incluidas las pruebas de autenticación y la prueba existente del endpoint `/health`.

Restore, build y ejecución completa de pruebas quedan pendientes de validación local posterior al merge de Prompt 010:

- `dotnet restore .\NuamExchange.sln`.
- `dotnet build .\NuamExchange.sln --no-restore`.
- `dotnet test .\NuamExchange.sln --no-build`.

## Pruebas manuales futuras: administración de usuarios

Posterior al merge, validar con base local de desarrollo y configuración JWT local:

1. Administrador consulta `GET /api/admin/users` y recibe listado paginado.
2. Administrador crea un usuario con rol Analista Tributario mediante `POST /api/admin/users`.
3. Analista Tributario intenta acceder a `GET /api/admin/users` y recibe `403 Forbidden`.
4. Crear usuario con correo duplicado y verificar `409 Conflict`.
5. Crear usuario con password inseguro y verificar rechazo seguro.
6. Cambiar estado activo/inactivo de un usuario mediante `PUT /api/admin/users/{id}`.
7. Restablecer contraseña mediante `POST /api/admin/users/{id}/reset-password`.
8. Intentar desactivar el último Administrador activo y verificar `409 Conflict`.
9. Revisar Auditoria para eventos `USER_CREATED`, `USER_UPDATED` y `USER_PASSWORD_RESET` sin contraseñas, hashes ni tokens.

## Evidencia de corrección — Prompt 013

- Fallo local heredado de Prompt 012: un JSON válido para `POST /api/admin/roles` con `name`, `description` string y `permissionIds` `[7, 8]` fue rechazado con `400` por deserialización de `description`.
- Corrección Prompt 013: se reforzó el contrato HTTP/Swagger de creación de roles y se agregaron pruebas de deserialización del DTO para `description` con texto y `null`, sin depender de SQL Server.
- Validación local obligatoria posterior al merge: iniciar la API en `Development`, autenticarse como Administrador, crear un rol personalizado real con permisos 7 y 8, confirmar `HTTP 201 Created`, consultar el detalle del rol creado y revisar Auditoria para `ROLE_CREATED`.

## Validaciones manuales futuras — Prompt 012

- Administrador consulta detalle de rol con `GET /api/admin/roles/{id}`.
- Administrador crea rol personalizado con `POST /api/admin/roles`.
- Administrador asigna permisos con `PUT /api/admin/roles/{id}/permissions`.
- Rol personalizado aparece en `GET /api/admin/roles`.
- Intento de modificar `Administrador` responde `409`.
- Intento de desactivar rol con usuarios activos responde `409`.
- Usuario `Analista Tributario` intenta crear rol y recibe `403`.
- Cambios de permisos se reflejan en `GET /api/auth/permissions`.
- Tabla `Auditoria` registra `ROLE_CREATED`, `ROLE_UPDATED` y `ROLE_PERMISSIONS_UPDATED`.

## Pruebas manuales futuras — Prompt 014

- Usuario `Administrador` consulta `GET /api/tax-classifications` y recibe `200`.
- Usuario `Analista Tributario` consulta `GET /api/tax-classifications` y recibe `200`.
- Usuario `Supervisor` consulta `GET /api/tax-classifications` y recibe `200`.
- Usuario sin token consulta `GET /api/tax-classifications` y recibe `401`.
- Usuario con rol no autorizado consulta `GET /api/tax-classifications` y recibe `403`.
- Consulta con filtro válido por `market`, `exercise`, `status` o `search` responde sin errores.
- Consulta con paginación válida (`page` y `pageSize`) responde metadatos consistentes.
- Consulta con parámetro de ordenamiento inválido responde `400`.
- Consulta de detalle inexistente en `GET /api/tax-classifications/{id}` responde `404`.
- Base sin registros devuelve listado vacío y opciones vacías.

## Validaciones manuales futuras — creación de Calificaciones Tributarias

- Administrador crea una calificación y recibe `201 Created`.
- Analista Tributario crea una calificación y recibe `201 Created`.
- Supervisor intenta crear una calificación y recibe `403 Forbidden`.
- Datos inválidos reciben `400 Bad Request`.
- `validTo` anterior a `validFrom` recibe `400 Bad Request`.
- El registro creado aparece en `GET /api/tax-classifications`.
- `GET /api/tax-classifications/{id}` retorna el registro creado.
- Existe historial inicial en `HistorialCalificacion` con `tipo_cambio = CREACION`.
- Existe auditoría `TAX_CLASSIFICATION_CREATED` en `Auditoria`.

## Evidencia de corrección — Prompt 016

- Fallo local real posterior al merge de Prompt 015: un JSON válido para `POST /api/tax-classifications` fue rechazado con `400 Bad Request` antes de crear la calificación, reportando error de binding sobre `description` y el campo `request` requerido.
- Corrección Prompt 016: se fijó explícitamente el contrato JSON de `CreateTaxClassificationRequest`, incluyendo `description` como `string` opcional y nullable, y se agregaron pruebas sin SQL Server para deserialización, contrato DTO y endpoint con servicio fake.
- Validación local posterior obligatoria: iniciar API en `Development`, autenticarse como Administrador, ejecutar el POST con el JSON documentado, confirmar `HTTP 201 Created`, confirmar cabecera `Location`, consultar listado y detalle, revisar `ClassificationHistory` y revisar Auditoria para `TAX_CLASSIFICATION_CREATED`.

## Validaciones manuales futuras — Prompt 017

- Administrador actualiza una calificación existente y recibe `HTTP 200 OK`.
- Analista Tributario actualiza una calificación existente y recibe `HTTP 200 OK`.
- Supervisor recibe `HTTP 403 Forbidden` al intentar editar.
- `PUT /api/tax-classifications/{id}` con id inexistente devuelve `404 Not Found`.
- `PUT` con fecha final anterior a la inicial devuelve `400 Bad Request`.
- `CreatorUserId`, `CreatedAt` y `Status` no cambian después de editar.
- `UpdatedAt` cambia luego de editar.
- `GET /api/tax-classifications/{id}/history` muestra creación y modificación.
- Auditoria incluye una entrada `TAX_CLASSIFICATION_UPDATED` para el registro actualizado.

## Validaciones manuales futuras — Copia de calificaciones tributarias

- Administrador copia una calificación existente.
- Analista Tributario copia una calificación existente.
- Supervisor recibe `403 Forbidden` al intentar copiar.
- Copia de id inexistente responde `404 Not Found`.
- Copia exitosa responde `201 Created` y cabecera `Location`.
- La copia tiene id distinto al origen.
- La copia conserva los campos editables del origen.
- La copia tiene nuevo creador, timestamps de servidor y estado inicial `VIGENTE`.
- El origen no cambia.
- La copia tiene historial propio con `CREACION`.
- Auditoría incluye `TAX_CLASSIFICATION_COPIED`.

## Casos de prueba agregados - Prompt 019

| Módulo | Objetivo | Datos de entrada | Pasos | Resultado esperado | Resultado obtenido | Prioridad | Responsable | Evidencia |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| API Calificaciones | Supervisor valida calificación en estado permitido | `id=7`, rol `Supervisor`, `decision=OBSERVADO` | POST `/api/tax-classifications/7/supervisor-validation` | `200 OK`, DTO con `status=OBSERVADA` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| API Calificaciones | Administrador valida calificación en estado permitido | `id=7`, rol `Administrador`, `decision=OBSERVADO` | POST `/api/tax-classifications/7/supervisor-validation` | `200 OK`, DTO seguro actualizado | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Seguridad | Analista Tributario no valida | rol `Analista Tributario` | POST endpoint de validación | `403 Forbidden` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Contrato | Decisión inválida | `decision=RECHAZADO` | POST endpoint de validación | `400 Bad Request` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Negocio | Transición no permitida | estado `ANULADA`, `decision=OBSERVADO` | Ejecutar servicio | `409 Conflict`, sin datos parciales | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| API Calificaciones | Id inexistente | `id=404` | POST endpoint de validación | `404 Not Found` | Cubierto por prueba automatizada | Media | Codex | `TaxClassificationSupervisorValidationTests` |
| Persistencia | Crear `ValidacionTributaria` | transición `VIGENTE -> OBSERVADA` | Ejecutar servicio con EF InMemory | Registro vinculado a calificación y usuario actor | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Persistencia | Cambiar estado permitido | `VIGENTE` + `OBSERVADO` | Ejecutar servicio | Estado final `OBSERVADA` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Persistencia | Historial registra cambio | transición válida | Ejecutar servicio | `tipo_cambio=OBSERVACION`, `Status`, anterior/nuevo | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Auditoría | Registrar operación | transición válida | Ejecutar servicio | `TAX_CLASSIFICATION_VALIDATED` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Consistencia | No dejar cambios parciales | transición inválida | Ejecutar servicio | Sin validación, historial ni auditoría | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
