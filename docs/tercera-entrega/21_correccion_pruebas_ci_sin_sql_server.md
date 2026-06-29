# 21. Corrección de pruebas CI sin SQL Server

## Falla previa

El workflow Backend CI inicial restauró y compiló correctamente, pero las pruebas HTTP/integración fallaron al crear fixtures basadas en `WebApplicationFactory<Program>` por ausencia de registro de `NuamExchange.Infrastructure.Persistence.NuamExchangeDbContext`.

La causa técnica fue que `AddInfrastructure` registra `NuamExchangeDbContext` solo cuando existe una connection string. GitHub Actions no debe depender de SQL Server, secretos ni una connection string real para ejecutar pruebas automatizadas.

## Corrección aplicada

Se agrega un helper interno exclusivo del proyecto `NuamExchange.Api.Tests` para configurar EF Core InMemory dentro de `ConfigureTestServices`.

El helper:

- elimina registros existentes de `DbContextOptions<NuamExchangeDbContext>`;
- elimina registros existentes de `NuamExchangeDbContext`;
- registra `NuamExchangeDbContext` con `UseInMemoryDatabase`;
- genera un nombre único de base por factory cuando no se entrega un nombre explícito;
- ignora la advertencia transaccional propia del proveedor InMemory;
- no requiere connection string ni SQL Server.

## Clases ajustadas

Las fixtures HTTP afectadas se actualizan para llamar el helper dentro de `ConfigureTestServices`, manteniendo los fakes existentes de servicios cuando correspondía:

- `AdminRolesEndpointBindingTests`.
- `TaxClassificationsEndpointBindingTests`.
- `TaxClassificationCopyTests`.
- `TaxClassificationSupervisorValidationTests`.
- `TaxClassificationBulkLoadXFactorTests`.
- `TaxClassificationBulkLoadXAmountTests`.
- `HealthEndpointTests`.

## Límites de la solución

No se agrega fallback InMemory en código productivo. La configuración InMemory queda limitada al proyecto de pruebas y se aplica solo desde `ConfigureTestServices`.

No se agregan SQL Server, migraciones, secretos, credenciales, despliegues, Plesk ni cambios bajo frontend. Tampoco se modifican entidades, Fluent API, endpoints, DTOs, políticas, JWT, login, usuarios, roles o permisos.

Si el entorno Cloud local no tiene `dotnet`, no se considera evidencia de pruebas pasadas en Cloud; la evidencia real será la ejecución publicada de GitHub Actions.
