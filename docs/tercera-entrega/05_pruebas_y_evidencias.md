# 05 — Pruebas y evidencias

## Prueba planificada

Se deja preparada una prueba de integración para `GET /health`, orientada a validar HTTP 200 y el contenido JSON esperado.

La prueba queda pendiente de validación local posterior al merge con .NET SDK 8.x disponible.

## Formato de evidencia

- Módulo.
- Objetivo.
- Datos de entrada.
- Pasos.
- Resultado esperado.
- Resultado obtenido.
- Prioridad.
- Responsable.
- Evidencia.
- Fecha.

## Registro Prompt 003 — Corrección de compilación de prueba `/health`

- Fecha: 2026-06-22.
- Validación local inicial de Prompt 002: ejecutada en Windows con .NET SDK 8.0.422.
- `dotnet restore .\NuamExchange.sln`: correcto en la validación local inicial.
- `dotnet build .\NuamExchange.sln --no-restore`: fallido por tipos de xUnit no resueltos en `HealthEndpointTests.cs` (`IClassFixture<>`, `FactAttribute` y `Fact`).
- `dotnet test .\NuamExchange.sln --no-build`: no ejecutado porque la DLL de pruebas no fue generada al fallar la compilación.
- Corrección abordada en Prompt 003: revisión de configuración del proyecto de pruebas y agregado de la directiva `using Xunit;` en la prueba de integración de `/health`.
- Resultado final de compilación y prueba: pendiente de validación local posterior al merge, porque Codex Cloud no dispone de SDK .NET en este entorno.

## Registro Prompt 004 — Corrección TypeScript/Vite y Fast Refresh

- Fecha: 2026-06-22.
- Validación local posterior a Prompt 003: ejecutada en Windows.
- Backend: `dotnet restore`, `dotnet build` y `dotnet test` aprobados; la suite reportó 1 prueba aprobada.
- Validación manual backend: `/health` y Swagger verificados correctamente.
- Frontend: `npm install` finalizó correctamente.
- Frontend: `npm run lint` finalizó sin errores, pero informó la advertencia `react-refresh/only-export-components` en `src/main.tsx` porque el archivo de montaje también declaraba el componente principal.
- Frontend: `npm run build` falló por configuración TypeScript obsoleta: `moduleResolution` resolvía como `node10` a partir de la configuración `"Node"`.
- Corrección abordada en Prompt 004: actualización de la resolución de módulos TypeScript para Vite moderno y separación del componente principal desde el archivo de montaje React.
- Build y lint final quedan pendientes de validación local posterior al merge de Prompt 004 si Codex Cloud no puede instalar o usar dependencias npm por restricciones externas del entorno.

## Registro Prompt 005 — Corrección de tipado Vite/CSS y artefactos TypeScript

- Fecha: 2026-06-22.
- Validación local posterior a Prompt 004: ejecutada en Windows.
- Frontend: `npm run lint` finalizó correctamente, sin advertencias ni errores.
- Frontend: `npm run build` falló por falta de tipado para la importación lateral de `./styles.css` en `src/main.tsx` (`TS2882: Cannot find module or type declarations for side-effect import`).
- Artefacto detectado durante validación local: `frontend/tsconfig.tsbuildinfo`, correspondiente a caché de TypeScript y no apto para versionamiento.
- Corrección abordada en Prompt 005: incorporación de la referencia estándar de tipos Vite para reconocer importaciones CSS y exclusión Git de artefactos `*.tsbuildinfo`.
- Build final queda pendiente de validación local posterior al merge de Prompt 005 si Codex Cloud no puede instalar o usar dependencias npm por restricciones externas del entorno.
