# 01. Arquitectura implementada

## Implementado en Prompt 006

Se incorporó el modelo de dominio persistente en `NuamExchange.Domain`, separado de detalles de Entity Framework Core. Las entidades se ubican en `Entities/`, usan nombres técnicos en inglés y mantienen navegaciones y nulabilidad coherentes para representar usuarios, roles, permisos, calificaciones tributarias, cargas masivas, validaciones, reportes, auditoría y respaldos.

Se incorporó `NuamExchange.Infrastructure` como capa de persistencia. La clase `NuamExchangeDbContext` expone los `DbSet` del modelo y aplica configuraciones Fluent API mediante `ApplyConfigurationsFromAssembly`. Las configuraciones mapean nombres físicos SQL Server en español, tipos de datos, claves primarias, claves foráneas, defaults, restricciones `CHECK`, relaciones e índices.

La API registra Infrastructure mediante `AddInfrastructure`. El `DbContext` solo se registra cuando existe una cadena no vacía `ConnectionStrings:NuamTributariaDb`; por lo tanto, el arranque de la API y `/health` no dependen de SQL Server cuando la cadena está vacía.

## Pendiente

Queda pendiente confirmar la instancia local de SQL Server, configurar una cadena de conexión de desarrollo fuera de los archivos con secretos, generar la migración inicial y crear localmente la base `NuamTributariaDB`. En Prompt 006 no se creó base real, no se ejecutó `database update` y no se generaron migraciones.
