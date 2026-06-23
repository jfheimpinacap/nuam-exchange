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
