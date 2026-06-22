# Nuam Exchange — Sistema de Gestión Tributaria

Nuam Exchange es la base técnica inicial de un sistema de gestión tributaria para una corredora de bolsa. Esta entrega prepara la estructura de frontend, backend, documentación y pruebas iniciales sin implementar módulos de negocio ni persistencia.

## Stack

- Frontend: React, TypeScript y Vite.
- Backend: ASP.NET Core Web API .NET 8.
- Base de datos futura: SQL Server 2022.
- ORM futuro: Entity Framework Core con proveedor SQL Server.
- Hosting futuro: Wirenet/Plesk.

## Estructura del repositorio

```text
backend-dotnet/
  NuamExchange.sln
  src/
    NuamExchange.Api/
    NuamExchange.Application/
    NuamExchange.Domain/
    NuamExchange.Infrastructure/
  tests/
    NuamExchange.Api.Tests/
frontend/
database/
docs/tercera-entrega/
```

## Requisitos locales

- .NET SDK 8.x.
- Node.js 20.19 o superior.

## Ejecución local

### API

```bash
cd backend-dotnet
dotnet restore NuamExchange.sln
dotnet run --project src/NuamExchange.Api/NuamExchange.Api.csproj
```

La API queda disponible en `http://localhost:5000` y expone el health check público en `http://localhost:5000/health`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

El frontend queda disponible en `http://localhost:5173` y usa proxy hacia la API local.

## Comandos de validación local

```bash
cd backend-dotnet
dotnet restore NuamExchange.sln
dotnet build NuamExchange.sln
dotnet test NuamExchange.sln
```

```bash
cd frontend
npm install
npm run lint
npm run build
```

## Estado de base de datos

SQL Server se configurará en una etapa posterior. Esta entrega no contiene migraciones, scripts SQL, entidades tributarias ni cadenas de conexión reales.

## Seguridad

No se deben subir credenciales, certificados, tokens, cadenas de conexión reales, archivos `.env` ni configuraciones productivas sensibles.
