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
