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
