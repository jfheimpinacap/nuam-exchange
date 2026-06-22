# Database

El modelo físico para SQL Server ahora existe como configuraciones Entity Framework Core en la capa `NuamExchange.Infrastructure`. Las tablas, columnas, claves, restricciones e índices están definidos por Fluent API y reservan la base futura `NuamTributariaDB`.

En este prompt no se generaron scripts SQL ejecutables, no se crearon migraciones, no se ejecutó `dotnet ef database update` y no se creó ninguna base de datos real.

La generación de la migración inicial y la creación local de `NuamTributariaDB` se realizarán posteriormente, cuando se confirme la instancia local de SQL Server. Este directorio no contiene comandos destructivos ni credenciales.
