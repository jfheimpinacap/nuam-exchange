# 20. Backend CI en GitHub Actions

Se agrega un workflow exclusivo para el backend .NET en `.github/workflows/backend-ci.yml` con nombre visible **Backend CI**.

## Alcance

- Ejecuta solamente la solución `backend-dotnet/NuamExchange.sln`.
- Corre en `ubuntu-latest` con `actions/checkout@v4` y `actions/setup-dotnet@v4`.
- Usa SDK .NET `8.0.x`.
- Define permisos mínimos `contents: read`.
- Limita cada ejecución a 15 minutos.
- Cancela ejecuciones redundantes mediante `concurrency`.
- Desactiva telemetría, logo y primera experiencia de .NET CLI mediante variables de entorno.

## Comandos automatizados

Desde `backend-dotnet` el workflow ejecuta pasos separados:

```bash
dotnet restore NuamExchange.sln
dotnet build NuamExchange.sln --configuration Release --no-restore
dotnet test NuamExchange.sln --configuration Release --no-build --logger "console;verbosity=normal"
```

## Triggers

El workflow se dispara por:

- Pull requests hacia `main`.
- Push a `main`.
- Ejecución manual con `workflow_dispatch`.

Los filtros `paths` incluyen:

- `backend-dotnet/**`.
- `global.json`.
- `.github/workflows/backend-ci.yml`.

La inclusión explícita de `global.json` evita omitir ejecuciones cuando cambia la versión del SDK .NET fijada para el repositorio.

## Exclusiones confirmadas

El workflow no incorpora frontend, Node, npm, SQL Server, `dotnet ef`, migraciones, secretos, despliegues, Plesk ni `pull_request_target`.

La evidencia real de restore, build y test en GitHub Actions queda asociada a las ejecuciones del workflow. Si el entorno Cloud local no dispone de `dotnet`, la ejecución de GitHub Actions será la evidencia ejecutable principal.
