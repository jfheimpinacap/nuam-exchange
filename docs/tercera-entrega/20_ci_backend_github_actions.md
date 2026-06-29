# CI backend con GitHub Actions

## Objetivo

Incorporar una validación automática para el backend .NET 8 de Nuam Exchange antes de integrar cambios a `main`. El workflow permite detectar fallas de restauración, compilación o pruebas automatizadas en Pull Requests y en pushes posteriores a `main`.

## Alcance exclusivo

El alcance de este workflow es exclusivamente el backend .NET ubicado en `backend-dotnet/` y la solución `backend-dotnet/NuamExchange.sln`.

No valida frontend en esta etapa. La integración de CI frontend queda fuera de este prompt y debe revisarse solo cuando se integre formalmente el repositorio de la compañera responsable.

## Eventos que disparan CI

El workflow `Backend CI` se ejecuta en:

- Pull Requests hacia `main` cuando cambian archivos bajo `backend-dotnet/**` o el propio workflow `.github/workflows/backend-ci.yml`.
- Push a `main` cuando cambian archivos bajo `backend-dotnet/**` o el propio workflow `.github/workflows/backend-ci.yml`.
- Ejecución manual mediante `workflow_dispatch`.

## Versión .NET usada

GitHub Actions configura el SDK .NET `8.0.x`, compatible con los proyectos `net8.0` de la solución backend.

## Comandos ejecutados

Desde el directorio `backend-dotnet`, el workflow ejecuta pasos separados:

```bash
dotnet restore NuamExchange.sln
dotnet build NuamExchange.sln --configuration Release --no-restore
dotnet test NuamExchange.sln --configuration Release --no-build --logger "console;verbosity=normal"
```

## Restricciones operativas y de seguridad

- No usa SQL Server real ni remoto.
- No ejecuta migraciones.
- No ejecuta `dotnet ef database update`.
- No usa secretos, credenciales ni cadenas de conexión reales.
- No publica artefactos, no despliega y no modifica ambientes.
- No inicia la API ni ejecuta validaciones manuales de endpoints.
- No valida frontend en esta etapa.

## Interpretación del resultado

Un resultado exitoso significa que GitHub Actions pudo restaurar dependencias, compilar la solución backend en configuración `Release` y ejecutar las pruebas automatizadas incluidas en `NuamExchange.sln`.

Un resultado fallido significa que al menos uno de esos pasos falló. El log del job indicará si el problema ocurrió en restore, build o test.

## Qué debe hacer el equipo ante una falla

Ante una falla del workflow:

1. No hacer merge del Pull Request.
2. Revisar el log del paso fallido en GitHub Actions.
3. Corregir la causa en un nuevo prompt correlativo.
4. Validar localmente restore, build y test antes de volver a abrir o actualizar el Pull Request.

## Limitaciones

- El workflow no reemplaza las validaciones manuales con API local en `http://localhost:5000`.
- El workflow no valida una instancia SQL Server real.
- El workflow no valida frontend.
- El workflow no publica ni despliega.

## Próxima evolución futura

Revisar CI de frontend solo cuando se integre formalmente el repositorio de la compañera responsable y se defina una estrategia de integración separada.
