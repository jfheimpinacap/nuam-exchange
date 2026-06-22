# 01 — Arquitectura implementada

## Estructura

- `frontend/`: aplicación React + TypeScript + Vite.
- `backend-dotnet/src/NuamExchange.Api/`: ASP.NET Core Web API y servidor de archivos estáticos.
- `backend-dotnet/src/NuamExchange.Application/`: capa de aplicación preparada.
- `backend-dotnet/src/NuamExchange.Domain/`: capa de dominio preparada.
- `backend-dotnet/src/NuamExchange.Infrastructure/`: capa de infraestructura preparada.
- `backend-dotnet/tests/NuamExchange.Api.Tests/`: pruebas xUnit de integración.

## Flujo esperado

Navegador → frontend React → API ASP.NET Core → SQL Server.

## Implementado en Prompt 002

- Solución .NET 8 y proyectos base.
- Endpoint público `GET /health`.
- Prueba de integración preparada para `/health`.
- Frontend temporal sin módulos de negocio.
- Documentación progresiva.

## Pendiente en prompts posteriores

- Autenticación, JWT, sesiones, roles y permisos.
- Entity Framework Core y SQL Server.
- Modelo tributario, entidades, migraciones y restricciones.
- Cargas X Factor y X Monto.
- Reportes, auditoría, archivos y despliegue.
