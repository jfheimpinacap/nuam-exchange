# Database

El modelo físico para SQL Server ahora existe como configuraciones Entity Framework Core en la capa `NuamExchange.Infrastructure`. Las tablas, columnas, claves, restricciones e índices están definidos por Fluent API y reservan la base futura `NuamTributariaDB`.

En este prompt no se generaron scripts SQL ejecutables, no se crearon migraciones, no se ejecutó `dotnet ef database update` y no se creó ninguna base de datos real.

La generación de la migración inicial y la creación local de `NuamTributariaDB` se realizarán posteriormente, cuando se confirme la instancia local de SQL Server. Este directorio no contiene comandos destructivos ni credenciales.

## Preparación de migraciones locales

Prompt 007 prepara el repositorio para generar posteriormente migraciones EF Core desde un entorno local con SQL Server configurado. No se ejecutaron estos comandos en Cloud ni se creó la base `NuamTributariaDB`.

El flujo previsto, una vez creado localmente el archivo ignorado `backend-dotnet/src/NuamExchange.Api/appsettings.Development.json`, es:

```text
cd backend-dotnet
dotnet tool restore
dotnet tool run dotnet-ef migrations add InitialCreate --project .\src\NuamExchange.Infrastructure --startup-project .\src\NuamExchange.Api --output-dir Persistence\Migrations
```

Ese comando se ejecutará solo después de configurar una cadena local válida para `NuamTributariaDB`. La migración generada debe revisarse antes de aplicar cambios a SQL Server. No se ejecutará `database update` sin confirmación explícita.
