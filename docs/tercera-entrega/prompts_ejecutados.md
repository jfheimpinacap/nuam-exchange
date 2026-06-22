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
