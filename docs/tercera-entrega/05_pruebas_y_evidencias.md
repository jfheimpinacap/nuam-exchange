# 05. Pruebas y evidencias

Prompt 006 agrega entidades de dominio, `NuamExchangeDbContext`, configuraciones Fluent API y proveedor SQL Server sin conectar una base real.

## Validaciones pendientes posteriores al merge

- `dotnet restore .\NuamExchange.sln`.
- `dotnet build .\NuamExchange.sln --no-restore`.
- `dotnet test .\NuamExchange.sln --no-build`.
- Inicio de API en Development sin connection string.
- Verificación de `/health`.
- Verificación de `/swagger` en Development.
- Futura migración local controlada para `NuamTributariaDB`.

No se generaron migraciones ni se ejecutó actualización de base de datos en Prompt 006.

## Validaciones futuras para migraciones locales

Prompt 007 agrega la preparación para migraciones locales, pero no genera migraciones ni crea bases de datos en Cloud. Las validaciones futuras obligatorias son:

- Ejecutar `dotnet tool restore` desde `backend-dotnet` para restaurar la herramienta local `dotnet-ef`.
- Crear localmente el archivo ignorado `appsettings.Development.json` con una cadena SQL Server válida para `NuamTributariaDB`.
- Generar de forma controlada la migración inicial con `dotnet-ef`.
- Revisar los archivos generados antes de aplicar cualquier cambio en SQL Server.
- Crear localmente la base `NuamTributariaDB` solo cuando corresponda.
- Verificar tablas, claves, restricciones e índices después de aplicar la migración en el entorno local autorizado.

## Actualización Prompt 009: pruebas de seguridad

### Pruebas unitarias agregadas

- BCrypt: generación de hash, diferencia contra password original, verificación correcta y rechazo de password incorrecto.
- JWT: creación de token no vacío, claims `sub`, `email` y rol, expiración futura y ausencia de password hash.

### Validaciones manuales futuras posteriores al merge

- Bootstrap de administrador local en Development.
- Login correcto.
- Login incorrecto con respuesta genérica.
- Expiración de token.
- Acceso autenticado a `/api/auth/me`.
- Acceso autenticado a `/api/auth/permissions`.
- Registro de auditoría para login exitoso, login fallido y bootstrap.

## Actualización Prompt 010: corrección global de xUnit en pruebas de autenticación

Después del merge de Prompt 009, la validación local inicial con .NET 8 falló al compilar el proyecto de pruebas porque `AuthenticationTests.cs` no resolvía los símbolos de xUnit (`FactAttribute`, `Fact` y aserciones como `Assert`). El resultado posterior de `dotnet test --no-build` no se considera evidencia válida para estas pruebas, porque pudo usar un DLL generado antes del fallo de compilación.

La corrección de Prompt 010 agrega un `GlobalUsings.cs` en el proyecto `NuamExchange.Api.Tests` con `global using Xunit;`, dejando disponible xUnit para todas las clases de prueba del proyecto, incluidas las pruebas de autenticación y la prueba existente del endpoint `/health`.

Restore, build y ejecución completa de pruebas quedan pendientes de validación local posterior al merge de Prompt 010:

- `dotnet restore .\NuamExchange.sln`.
- `dotnet build .\NuamExchange.sln --no-restore`.
- `dotnet test .\NuamExchange.sln --no-build`.

## Pruebas manuales futuras: administración de usuarios

Posterior al merge, validar con base local de desarrollo y configuración JWT local:

1. Administrador consulta `GET /api/admin/users` y recibe listado paginado.
2. Administrador crea un usuario con rol Analista Tributario mediante `POST /api/admin/users`.
3. Analista Tributario intenta acceder a `GET /api/admin/users` y recibe `403 Forbidden`.
4. Crear usuario con correo duplicado y verificar `409 Conflict`.
5. Crear usuario con password inseguro y verificar rechazo seguro.
6. Cambiar estado activo/inactivo de un usuario mediante `PUT /api/admin/users/{id}`.
7. Restablecer contraseña mediante `POST /api/admin/users/{id}/reset-password`.
8. Intentar desactivar el último Administrador activo y verificar `409 Conflict`.
9. Revisar Auditoria para eventos `USER_CREATED`, `USER_UPDATED` y `USER_PASSWORD_RESET` sin contraseñas, hashes ni tokens.

## Evidencia de corrección — Prompt 013

- Fallo local heredado de Prompt 012: un JSON válido para `POST /api/admin/roles` con `name`, `description` string y `permissionIds` `[7, 8]` fue rechazado con `400` por deserialización de `description`.
- Corrección Prompt 013: se reforzó el contrato HTTP/Swagger de creación de roles y se agregaron pruebas de deserialización del DTO para `description` con texto y `null`, sin depender de SQL Server.
- Validación local obligatoria posterior al merge: iniciar la API en `Development`, autenticarse como Administrador, crear un rol personalizado real con permisos 7 y 8, confirmar `HTTP 201 Created`, consultar el detalle del rol creado y revisar Auditoria para `ROLE_CREATED`.

## Validaciones manuales futuras — Prompt 012

- Administrador consulta detalle de rol con `GET /api/admin/roles/{id}`.
- Administrador crea rol personalizado con `POST /api/admin/roles`.
- Administrador asigna permisos con `PUT /api/admin/roles/{id}/permissions`.
- Rol personalizado aparece en `GET /api/admin/roles`.
- Intento de modificar `Administrador` responde `409`.
- Intento de desactivar rol con usuarios activos responde `409`.
- Usuario `Analista Tributario` intenta crear rol y recibe `403`.
- Cambios de permisos se reflejan en `GET /api/auth/permissions`.
- Tabla `Auditoria` registra `ROLE_CREATED`, `ROLE_UPDATED` y `ROLE_PERMISSIONS_UPDATED`.

## Pruebas manuales futuras — Prompt 014

- Usuario `Administrador` consulta `GET /api/tax-classifications` y recibe `200`.
- Usuario `Analista Tributario` consulta `GET /api/tax-classifications` y recibe `200`.
- Usuario `Supervisor` consulta `GET /api/tax-classifications` y recibe `200`.
- Usuario sin token consulta `GET /api/tax-classifications` y recibe `401`.
- Usuario con rol no autorizado consulta `GET /api/tax-classifications` y recibe `403`.
- Consulta con filtro válido por `market`, `exercise`, `status` o `search` responde sin errores.
- Consulta con paginación válida (`page` y `pageSize`) responde metadatos consistentes.
- Consulta con parámetro de ordenamiento inválido responde `400`.
- Consulta de detalle inexistente en `GET /api/tax-classifications/{id}` responde `404`.
- Base sin registros devuelve listado vacío y opciones vacías.

## Validaciones manuales futuras — creación de Calificaciones Tributarias

- Administrador crea una calificación y recibe `201 Created`.
- Analista Tributario crea una calificación y recibe `201 Created`.
- Supervisor intenta crear una calificación y recibe `403 Forbidden`.
- Datos inválidos reciben `400 Bad Request`.
- `validTo` anterior a `validFrom` recibe `400 Bad Request`.
- El registro creado aparece en `GET /api/tax-classifications`.
- `GET /api/tax-classifications/{id}` retorna el registro creado.
- Existe historial inicial en `HistorialCalificacion` con `tipo_cambio = CREACION`.
- Existe auditoría `TAX_CLASSIFICATION_CREATED` en `Auditoria`.

## Evidencia de corrección — Prompt 016

- Fallo local real posterior al merge de Prompt 015: un JSON válido para `POST /api/tax-classifications` fue rechazado con `400 Bad Request` antes de crear la calificación, reportando error de binding sobre `description` y el campo `request` requerido.
- Corrección Prompt 016: se fijó explícitamente el contrato JSON de `CreateTaxClassificationRequest`, incluyendo `description` como `string` opcional y nullable, y se agregaron pruebas sin SQL Server para deserialización, contrato DTO y endpoint con servicio fake.
- Validación local posterior obligatoria: iniciar API en `Development`, autenticarse como Administrador, ejecutar el POST con el JSON documentado, confirmar `HTTP 201 Created`, confirmar cabecera `Location`, consultar listado y detalle, revisar `ClassificationHistory` y revisar Auditoria para `TAX_CLASSIFICATION_CREATED`.

## Validaciones manuales futuras — Prompt 017

- Administrador actualiza una calificación existente y recibe `HTTP 200 OK`.
- Analista Tributario actualiza una calificación existente y recibe `HTTP 200 OK`.
- Supervisor recibe `HTTP 403 Forbidden` al intentar editar.
- `PUT /api/tax-classifications/{id}` con id inexistente devuelve `404 Not Found`.
- `PUT` con fecha final anterior a la inicial devuelve `400 Bad Request`.
- `CreatorUserId`, `CreatedAt` y `Status` no cambian después de editar.
- `UpdatedAt` cambia luego de editar.
- `GET /api/tax-classifications/{id}/history` muestra creación y modificación.
- Auditoria incluye una entrada `TAX_CLASSIFICATION_UPDATED` para el registro actualizado.

## Validaciones manuales futuras — Copia de calificaciones tributarias

- Administrador copia una calificación existente.
- Analista Tributario copia una calificación existente.
- Supervisor recibe `403 Forbidden` al intentar copiar.
- Copia de id inexistente responde `404 Not Found`.
- Copia exitosa responde `201 Created` y cabecera `Location`.
- La copia tiene id distinto al origen.
- La copia conserva los campos editables del origen.
- La copia tiene nuevo creador, timestamps de servidor y estado inicial `VIGENTE`.
- El origen no cambia.
- La copia tiene historial propio con `CREACION`.
- Auditoría incluye `TAX_CLASSIFICATION_COPIED`.

## Casos de prueba agregados - Prompt 019

| Módulo | Objetivo | Datos de entrada | Pasos | Resultado esperado | Resultado obtenido | Prioridad | Responsable | Evidencia |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| API Calificaciones | Supervisor valida calificación en estado permitido | `id=7`, rol `Supervisor`, `decision=OBSERVADO` | POST `/api/tax-classifications/7/supervisor-validation` | `200 OK`, DTO con `status=OBSERVADA` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| API Calificaciones | Administrador valida calificación en estado permitido | `id=7`, rol `Administrador`, `decision=OBSERVADO` | POST `/api/tax-classifications/7/supervisor-validation` | `200 OK`, DTO seguro actualizado | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Seguridad | Analista Tributario no valida | rol `Analista Tributario` | POST endpoint de validación | `403 Forbidden` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Contrato | Decisión inválida | `decision=RECHAZADO` | POST endpoint de validación | `400 Bad Request` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Negocio | Transición no permitida | estado `ANULADA`, `decision=OBSERVADO` | Ejecutar servicio | `409 Conflict`, sin datos parciales | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| API Calificaciones | Id inexistente | `id=404` | POST endpoint de validación | `404 Not Found` | Cubierto por prueba automatizada | Media | Codex | `TaxClassificationSupervisorValidationTests` |
| Persistencia | Crear `ValidacionTributaria` | transición `VIGENTE -> OBSERVADA` | Ejecutar servicio con EF InMemory | Registro vinculado a calificación y usuario actor | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Persistencia | Cambiar estado permitido | `VIGENTE` + `OBSERVADO` | Ejecutar servicio | Estado final `OBSERVADA` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Persistencia | Historial registra cambio | transición válida | Ejecutar servicio | `tipo_cambio=OBSERVACION`, `Status`, anterior/nuevo | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Auditoría | Registrar operación | transición válida | Ejecutar servicio | `TAX_CLASSIFICATION_VALIDATED` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |
| Consistencia | No dejar cambios parciales | transición inválida | Ejecutar servicio | Sin validación, historial ni auditoría | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationSupervisorValidationTests` |

## Casos Prompt 020 — Carga Masiva X Factor

| Módulo | Objetivo | Datos de entrada | Pasos | Resultado esperado | Resultado obtenido | Prioridad | Responsable | Evidencia |
|---|---|---|---|---|---|---|---|---|
| API Carga X Factor | Administrador carga CSV X Factor válido | JWT Administrador, CSV UTF-8 con `file` | POST multipart a `/api/tax-classifications/bulk-loads/x-factor` | `200 OK`, conteos correctos | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| API Carga X Factor | Analista Tributario carga CSV X Factor válido | JWT Analista, CSV válido | POST multipart | `200 OK` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Seguridad | Supervisor recibe 403 | JWT Supervisor, CSV válido | POST multipart | `403 Forbidden` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| API Carga X Factor | Archivo vacío responde 400 | `file` vacío `.csv` | POST multipart | `400 Bad Request` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| API Carga X Factor | Encabezado inválido responde 400 | CSV con header distinto | POST multipart | `400 Bad Request` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Fila válida actualiza `AppliedFactor` | `BOLSA;NUAM-1;2026;1.25000000` | Ejecutar servicio con EF InMemory | Solo cambia factor y `UpdatedAt` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Fila inválida registra error sin modificar calificación | Decimal inválido | Ejecutar servicio | Error de fila, sin historial ni auditoría tributaria para esa fila | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Coincidencia inexistente registra error | Instrumento inexistente | Ejecutar servicio | `NOT_FOUND`, sin creación tributaria | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Coincidencia ambigua registra error | Dos calificaciones con misma identidad lógica | Ejecutar servicio | `AMBIGUOUS_MATCH`, sin modificación | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Duplicado en archivo registra error | Identidad repetida | Ejecutar servicio | Segunda ocurrencia `DUPLICATE_ROW` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Decimal inválido con identidad no procesada | Fila `BAD-FIRST` con `appliedFactor=bad` antes de su primera fila válida | Ejecutar servicio | Error `INVALID_APPLIED_FACTOR`, sin actualización, historial ni auditoría por esa fila | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Fila válida posterior a fila inválida de la misma identidad | Misma identidad `BAD-FIRST` después del decimal inválido | Ejecutar servicio | Se procesa como primera válida y actualiza solo `AppliedFactor`/`UpdatedAt` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Servicio Carga X Factor | Duplicado posterior a identidad procesada | `NUAM-1` repetido después de una actualización exitosa | Ejecutar servicio | Segunda ocurrencia `DUPLICATE_ROW`, sin historial ni auditoría adicional para esa fila | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Historial | Historial registra tipo permitido | Fila aplicada | Consultar `ClassificationHistory` | `MODIFICACION` y `AppliedFactor` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Auditoría | Auditoría registra acción implementada | Fila aplicada | Consultar `AuditLog` | `TAX_CLASSIFICATION_FACTOR_BULK_UPDATED` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXFactorTests` |
| Consistencia | Sin cambios parciales ante fallo inesperado | Falla de infraestructura durante transacción | Ejecutar con proveedor transaccional en validación posterior | Rollback completo | Pendiente de validación local con proveedor relacional; la implementación usa transacción EF Core | Alta | Equipo local | Validación posterior obligatoria |
## Actualización Prompt 022 — Corrección de compilación X Factor

Se corrigió la compilación de la prueba automatizada de Carga Masiva X Factor: la validación de los factores de filas ambiguas compara `AppliedFactor` como `decimal?` contra una colección `decimal?[]`, compatible con la nulabilidad real de la entidad `TaxClassification`.

La cobertura automatizada conserva la verificación fuerte de los factores aplicados y los casos de servicio para:

- fila válida que actualiza `AppliedFactor`;
- `NOT_FOUND`;
- `AMBIGUOUS_MATCH`;
- `INVALID_APPLIED_FACTOR`;
- `DUPLICATE_ROW`;
- fila válida posterior a una fila inválida con la misma identidad, confirmando que una fila inválida no consume la identidad procesable.

No se modificaron migraciones, entidades, Fluent API, snapshot, modelo físico, frontend, roles, permisos, JWT ni políticas. La validación local posterior obligatoria sigue siendo restaurar, compilar y ejecutar pruebas sobre el binario recompilado antes de continuar con la prueba manual de Carga Masiva X Factor.

## Casos agregados — Prompt 023 Carga Masiva X Monto

| Módulo | Objetivo | Datos de entrada | Pasos | Resultado esperado | Resultado obtenido | Prioridad | Responsable | Evidencia |
|---|---|---|---|---|---|---|---|---|
| API Carga X Monto | Administrador/Analista carga CSV válido | JWT con rol permitido, multipart `file` CSV UTF-8 | POST a `/api/tax-classifications/bulk-loads/x-amount` | `200 OK` con conteos | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| API Carga X Monto | Supervisor no ejecuta carga | JWT Supervisor | POST multipart | `403 Forbidden` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| API Carga X Monto | Sin JWT no ejecuta carga | Sin autenticación | POST multipart | `401 Unauthorized` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| API Carga X Monto | Rechazar archivo faltante/vacío/no CSV/header inválido/JSON | Multipart vacío, CSV vacío, TXT, encabezado incorrecto, JSON | POST al endpoint | `400 Bad Request` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Fila válida actualiza `ReferenceAmount` | `BOLSA;NUAM-1;2026;1.2500` | Ejecutar servicio con EF InMemory | Solo cambia monto y `UpdatedAt` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Monto inválido | `referenceAmount=bad` o exceso de escala/precisión | Ejecutar servicio | Error `INVALID_REFERENCE_AMOUNT`, sin cambios tributarios | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Coincidencia inexistente | Identidad sin calificación | Ejecutar servicio | Error `NOT_FOUND`, sin creación | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Coincidencia ambigua | Dos calificaciones con misma identidad | Ejecutar servicio | Error `AMBIGUOUS_MATCH`, sin cambios | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Duplicado posterior a fila válida | Misma identidad dos veces | Ejecutar servicio | Segunda fila `DUPLICATE_ROW` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Fila válida posterior a fila inválida | Misma identidad con monto inválido y luego válido | Ejecutar servicio | Fila válida se procesa como primera aplicada | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Servicio Carga X Monto | Preservación de campos | Calificación con `Status`, `AppliedFactor`, creador y fechas | Ejecutar carga válida | Campos preservados salvo `ReferenceAmount` y `UpdatedAt` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Historial | Registrar modificación permitida | Fila aplicada | Consultar `ClassificationHistory` | `MODIFICACION`, `ReferenceAmount` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Auditoría | Registrar acción de monto | Fila aplicada | Consultar `AuditLog` | `TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED` | Cubierto por prueba automatizada | Alta | Codex | `TaxClassificationBulkLoadXAmountTests` |
| Transacción | Rollback ante error inesperado | Falla de infraestructura simulable | Ejecutar servicio | Sin persistencia parcial | Cubierto por transacción del servicio y pendiente de validación local ampliada | Alta | Codex | Revisión estática + pruebas |

## Actualización Prompt 024 — Corrección de fakes tras Carga Masiva X Monto

Se corrigieron implementaciones fake de pruebas que quedaron desactualizadas al extender `ITaxClassificationCommandService` con `BulkLoadXAmountAsync` para Carga Masiva X Monto.

La actualización cubre los dobles manuales usados por pruebas de copia, binding JSON de endpoints y validación supervisora. En esos fakes el método de X Monto se implementa con `NotSupportedException`, de forma explícita y segura, porque esos fixtures no ejercitan el endpoint ni el servicio de Carga Masiva X Monto. Los fakes específicos de Carga Masiva X Factor y Carga Masiva X Monto ya exponen comportamientos coherentes con sus contratos respectivos.

No se eliminaron pruebas ni se redujo cobertura. No se modificaron migraciones, entidades, Fluent API, snapshot, modelo físico, frontend, roles, permisos, JWT ni políticas. La validación local posterior obligatoria sigue siendo restaurar, compilar y ejecutar pruebas sobre binarios recompilados antes de continuar con la prueba manual de Carga Masiva X Monto.

## Prompt 025 — Consulta de cargas masivas

Casos agregados para validar endpoints de solo lectura con EF Core InMemory y sin SQL Server real:

| Área | Caso | Resultado esperado |
|---|---|---|
| Autorización | Administrador consulta `/api/bulk-loads` | `200 OK` |
| Autorización | Analista Tributario consulta `/api/bulk-loads` | `200 OK` |
| Autorización | Supervisor consulta `/api/bulk-loads` | `200 OK` |
| Autorización | Sin JWT consulta `/api/bulk-loads` | `401 Unauthorized` |
| Listado | Listado paginado con cargas existentes | `200 OK`, `items`, `page`, `pageSize`, `totalCount`, `totalPages` |
| Listado | Listado vacío | `200 OK` con `items` vacío |
| Listado | Filtros por `uploadType` y `status` | Devuelve solo cargas coincidentes |
| Listado | `page`, `pageSize` o `sortBy` inválidos | `400 Bad Request` |
| Seguridad | Respuesta de listado | No expone ruta física ni hash de archivo |
| Resumen | Id existente | `200 OK` con resumen seguro |
| Resumen | Id inexistente | `404 Not Found` |
| Detalles | Consulta de filas de una carga | Solo detalles de la carga solicitada |
| Detalles | Carga sin detalles | `200 OK` con colección vacía |
| Detalles | Id inexistente | `404 Not Found` |
| Errores | Consulta de errores de una carga | Solo errores de la carga solicitada |
| Errores | Carga sin errores | `200 OK` con colección vacía |
| Errores | Id inexistente | `404 Not Found` |
| Integridad | Consultas de listado, resumen, detalles y errores | No modifican `UploadFile`, `BulkUploadDetail`, `BulkUploadError` ni `TaxClassification` |

## Actualización Prompt 026 — Corrección de consultas y pruebas de Cargas Masivas

Se corrigió la causa real de las fallas de validación local posteriores a Prompt 025: el fixture de `BulkLoadQueryTests` configuraba EF Core InMemory con `Guid.NewGuid().ToString()` dentro del callback de `AddDbContext`, por lo que el contexto usado para sembrar datos y el contexto usado por la API podían resolver nombres distintos de base InMemory. Como consecuencia, las consultas de listado, resumen, detalles y errores se ejecutaban contra una base vacía, produciendo listado sin items, `404 Not Found` para una carga existente y respuestas no compatibles con las assertions esperadas.

La corrección captura un único nombre de base InMemory por factory de prueba y lo reutiliza tanto para el seed como para los contextos de consulta. Además, se reforzaron las pruebas para confirmar explícitamente:

- carga existente con filtros reales devuelve `200 OK` e `items` no nulo;
- carga inexistente conserva `404 Not Found`;
- detalles y errores devuelven `PagedResult` con `items` no nulo;
- cargas sin detalles ni errores devuelven colecciones vacías no nulas;
- detalles y errores permanecen aislados por `UploadFileId`;
- paginación conserva `totalCount` y `totalPages`;
- no se exponen `FilePath`, `FileHash` ni rutas físicas;
- las consultas siguen siendo de solo lectura sobre `UploadFile`, `BulkUploadDetail`, `BulkUploadError` y `TaxClassification`.

También se corrigió la advertencia xUnit2012 sustituyendo la assertion basada en `Assert.False(...Any(...))` por `Assert.DoesNotContain(...)`, sin desactivar analyzers ni debilitar la intención de validar ausencia de cambios rastreados.

No se modificaron entidades, Fluent API, migraciones, snapshot, modelo físico, frontend, JWT, roles, permisos ni políticas. No se usó SQL Server real ni credenciales reales.

## Prompt 027 — Matriz de pruebas de reporte tributario y CSV

| Área | Evidencia cubierta |
| --- | --- |
| Filtros | market, instrumentCode, taxPeriod, status, classificationType y currency validados en consulta compartida. |
| Paginación | page/pageSize y totalPages sobre universo filtrado. |
| Resumen agregado | totales por estado, tipo, factor aplicado y monto de referencia. |
| Moneda | ReferenceAmount se suma por currency y no mezcla monedas. |
| CSV | UTF-8 BOM, encabezado, delimitador punto y coma y content disposition seguro. |
| Límite | Más de 10000 filas devuelve 400 sin archivo parcial. |
| Escape CSV | Punto y coma, comillas y saltos de línea se escapan. |
| Formula Injection | Textos que comienzan con =, +, - o @ se neutralizan con apóstrofo. |
| Roles | Administrador, Analista Tributario y Supervisor usan TaxClassificationRead; sin JWT devuelve 401. |
| Integridad | Consulta y exportación no modifican calificaciones ni crean cargas, errores, detalles, historiales, auditoría ni ReporteTributario. |

## Actualización Prompt 028 — Corrección de fixture y pruebas de Reporte Tributario CSV

Se corrigió el fallo posterior a Prompt 027 en `TaxReportQueryTests.JsonEndpoint_ReturnsSafeContractAndCsvHeaders`, donde la prueba esperaba tres calificaciones tributarias filtradas por `market=BOLSA`, pero el endpoint JSON recibía cero.

La causa real inspeccionada fue el fixture de integración: `CreateFactory` configuraba EF Core InMemory con `Guid.NewGuid().ToString()` dentro del callback de `AddDbContext`. Ese callback puede ejecutarse más de una vez, por lo que el contexto usado para sembrar datos y el contexto resuelto por la API podían apuntar a bases InMemory distintas. El seed sí ejecutaba `SaveChanges`, pero no necesariamente sobre la misma base que consumía el cliente HTTP.

La corrección captura un único nombre de base InMemory por `WebApplicationFactory` y reutiliza ese nombre para el seed y para los contextos de la API. La prueba ahora confirma explícitamente que la base visible desde `factory.Services` contiene las cuatro calificaciones sembradas antes de consultar el endpoint, y mantiene la expectativa válida de tres filas para `market=BOLSA`.

Se reforzó la cobertura del reporte tributario para validar:

- `200 OK`, `items` no nulo, `summary` no nulo, `totalCount`, `totalPages` y resumen sobre el universo filtrado completo;
- filtros válidos que devuelven solo las filas esperadas;
- listado vacío con `200 OK`, `items` vacío no nulo y `summary` no nulo;
- CSV con BOM UTF-8, encabezado, delimitador punto y coma, mismo filtro lógico, escape de comillas/punto y coma/saltos de línea y neutralización de Formula Injection;
- exclusión de campos internos como rutas físicas o claims;
- solo lectura: consultar JSON y exportar CSV no modifica `TaxClassification` ni crea `UploadFile`, `BulkUploadDetail`, `BulkUploadError`, `ClassificationHistory`, `AuditLog` ni `TaxReport`.

No se debilitó la cobertura ni se cambió una expectativa válida de `3` a `0`. No se modificaron entidades, Fluent API, migraciones, snapshot, modelo físico, frontend, JWT, roles, permisos ni políticas. No se usó SQL Server real ni credenciales reales.

## Prompt 029 — Matriz de pruebas de consulta de auditoría tributaria

| Área | Caso | Resultado esperado | Cobertura |
|---|---|---|---|
| Roles | Administrador consulta | `200 OK` | `TaxAuditQueryTests` |
| Roles | Analista Tributario consulta | `200 OK` | `TaxAuditQueryTests` |
| Roles | Supervisor consulta | `200 OK` | `TaxAuditQueryTests` |
| Seguridad | Sin JWT | `401 Unauthorized` | `TaxAuditQueryTests` |
| Listado | Respuesta paginada | `items` no nulo, `totalCount` y `totalPages` correctos | `TaxAuditQueryTests` |
| Listado | Vacío | `200 OK`, `items=[]`, `totalPages=0` | `TaxAuditQueryTests` |
| Filtros | Acción tributaria | Solo acción permitida | `TaxAuditQueryTests` |
| Filtros | Calificación tributaria | Solo `registro_afectado_id` solicitado | `TaxAuditQueryTests` |
| Filtros | Fechas | Solo rango solicitado | `TaxAuditQueryTests` |
| Paginación | `page`/`pageSize` inválidos | `400 Bad Request` | `TaxAuditQueryTests` |
| Ordenamiento | `sortBy` permitido/no permitido | permitido funciona; no permitido `400` | `TaxAuditQueryTests` |
| Alcance | Eventos no tributarios | No aparecen en listado | `TaxAuditQueryTests` |
| Detalle | Evento tributario existente | `200 OK` con DTO seguro | `TaxAuditQueryTests` |
| Detalle | Id inexistente | `404 Not Found` | `TaxAuditQueryTests` |
| Detalle | Id no tributario | `404 Not Found` | `TaxAuditQueryTests` |
| Campos excluidos | IP, ruta, hash, email, password, claim, connection string | No expuestos | `TaxAuditQueryTests` |
| Integridad | Ausencia de modificaciones | No cambia auditoría ni datos operacionales | `TaxAuditQueryTests` |
## Evidencia de corrección — Prompt 030

- Durante la validación local posterior a Prompt 029 se detectó el error `CS0246` en `backend-dotnet/src/NuamExchange.Application/TaxAudits/TaxAuditDtos.cs`: el tipo genérico `PagedResult<>` no se resolvía desde el namespace de auditoría tributaria.
- La causa real fue una directiva `using` faltante hacia el namespace canónico `NuamExchange.Application.TaxClassifications`, donde ya existe `PagedResult<T>` como contrato paginado compartido por consultas de calificaciones tributarias y cargas masivas.
- La corrección mínima aplicada fue reutilizar ese `PagedResult<T>` existente agregando la referencia de namespace requerida en los DTOs de Auditoría Tributaria.
- No se creó una segunda implementación de `PagedResult<T>`, no se movieron clases entre capas y no se agregó una referencia desde Application hacia API.
- No se modificó la lógica funcional de auditoría tributaria, sus filtros, autorización, endpoints ni contrato HTTP; se mantiene el contrato paginado con `items`, `page`, `pageSize`, `totalCount` y `totalPages`.
- Validación local posterior obligatoria: ejecutar `dotnet restore ./backend-dotnet/NuamExchange.sln`, `dotnet build ./backend-dotnet/NuamExchange.sln --no-restore` y `dotnet test ./backend-dotnet/NuamExchange.sln --no-build` sobre binarios recompilados.

## Evidencia de corrección — Prompt 031

- Después de Prompt 030 se detectó en validación manual que `GET /api/tax-audits?page=1&pageSize=100` respondía `HTTP 503` aunque la base estaba operativa para login, consulta de usuario e inserción de auditoría.
- La causa técnica fue el uso de `IReadOnlySet<string>.Contains` dentro de una consulta `IQueryable` de EF Core para filtrar acciones tributarias permitidas; esa forma podía no ser traducible por SQL Server antes de `CountAsync`, `ToListAsync` o `SingleOrDefaultAsync`.
- La corrección mantiene una única fuente de verdad en `TaxAuditRules`: un arreglo estático con las seis acciones tributarias reales y un `AllowedActions` derivado para validación en memoria.
- El alcance tributario cerrado se conserva: entidad `CalificacionTributaria`, `registro_afectado_id` informado y acción dentro de la lista cerrada permitida.
- Se agregó validación de traducción SQL mediante `ToQueryString()` con proveedor relacional SQL Server y connection string local ficticia, sin abrir conexión, sin base real y sin migraciones.
- El comportamiento seguro `HTTP 503` se conserva sin exponer stack trace, connection string ni detalles internos; además, el controlador registra la excepción del lado servidor con `ILogger<TaxAuditsController>`.

## Prompt 032 — Matriz de revisión documental del módulo de Respaldos

| Área revisada | Evidencia | Resultado |
| --- | --- | --- |
| Entidad | `BackupRecord` en dominio | Existe modelo persistente de metadatos parciales. |
| DbContext | `BackupRecords` en `NuamExchangeDbContext` | Existe `DbSet`; no implica endpoint ni operación. |
| Fluent API | `BackupRecordConfiguration` | Tabla `Respaldo`, CHECK de tipo/estado, FK opcional a `Usuario`. |
| Migración inicial | `20260622200947_InitialCreate` | Tabla, PK, FK, CHECK e índice versionados previamente. |
| Snapshot | `NuamExchangeDbContextModelSnapshot` | Modelo EF Core consistente con migración inicial. |
| Código funcional | Controllers, servicios, DTOs, jobs y frontend | No se encontraron operaciones de respaldo o restauración. |
| Scripts y comandos | `BACKUP DATABASE`, `RESTORE DATABASE`, `sqlcmd`, comandos de SO | No se encontraron implementaciones relacionadas. |
| Seguridad | Políticas/JWT/roles/permisos | No se modificaron; no existe autorización aprobada para backup/restore. |
| Documentación | Modelo, seguridad, evidencias y prompt ejecutado | Se documentó decisión de no implementar operaciones reales. |

Confirmación de ausencia de operaciones: la revisión global de `Backup`, `Respaldo`, `Restore` y `Recovery` encontró únicamente el modelo persistente, documentación, comandos generales de `dotnet restore` y el permiso semilla `backups.read`; no se encontraron endpoints, servicios funcionales, scripts ni automatizaciones de backup o restore.

Validaciones futuras requeridas antes de implementar operaciones reales:

- Aprobar propietario operativo, ambiente permitido y base objetivo.
- Definir cuenta técnica de privilegios mínimos.
- Definir almacenamiento, cifrado, gestión de claves y nombres de archivo.
- Definir retención, eliminación segura e integridad.
- Definir monitoreo, auditoría, recuperación, restauración y pruebas de restore.
- Definir flujo de aprobación, doble aprobación y manejo de incidentes.
- Definir separación entre desarrollo, pruebas y producción.
- Aprobar política y roles antes de cualquier consulta de metadatos.

No se realizaron migraciones, cambios de modelo físico, cambios de entidades, cambios de Fluent API, cambios de snapshot, cambios de frontend, endpoints, servicios funcionales ni datos de prueba.

## Prompt 033 — Evidencias de consulta segura de metadatos de respaldos

| Cobertura | Evidencia esperada |
|---|---|
| Autorización Administrador | `GET /api/backup-metadata` responde `200 OK`. |
| Autorización Supervisor | `GET /api/backup-metadata` responde `403 Forbidden`. |
| Autorización Analista Tributario | `GET /api/backup-metadata` responde `403 Forbidden`. |
| No autenticado | `GET /api/backup-metadata` responde `401 Unauthorized`. |
| Listado | Respuesta paginada con `items`, `page`, `pageSize`, `totalCount` y `totalPages`. |
| Orden por defecto | `occurredAt desc` y luego `id desc`. |
| Filtros | `backupType`, `status`, `dateFrom` y `dateTo` aplicados antes de conteo y paginación. |
| Parámetros inválidos | `page`, `pageSize`, `sortBy`, `sortDirection` y rango de fechas inválido responden `400`. |
| Detalle existente | `GET /api/backup-metadata/{id}` responde `200 OK`. |
| Detalle inexistente | `GET /api/backup-metadata/{id-inexistente}` responde `404 Not Found`. |
| Exclusión de campos sensibles | No se devuelven rutas, `BackupPath`, `ruta_respaldo`, observaciones, usuarios, emails, tokens, hashes ni tamaños. |
| Solo lectura | Las consultas no modifican `Respaldo`, `Auditoria`, calificaciones tributarias, cargas masivas ni usuarios. |

Las pruebas usan exclusivamente EF Core InMemory dentro del proyecto de pruebas. No usan archivos físicos, bases externas, `NuamTributariaDB_Dev`, migraciones, SQL Server remoto, Plesk ni credenciales reales.

## Evidencia de corrección — Prompt 034

- Durante la validación local posterior a Prompt 033 se detectó un fallo de compilación en `BackupMetadataQueryTests.cs`: la fixture de la prueba de consulta de metadatos de respaldos inicializaba `UploadTemplate.Name` y `UploadTemplate.RequiredFieldsJson`, propiedades inexistentes en la entidad real.
- La inspección confirmó que `UploadTemplate` define `UploadType`, `TemplateName`, `Description`, `RequiredColumns`, `AllowedFormat`, `TemplateVersion`, `IsActive`, `CreatedAt` y la navegación `UploadFiles`.
- La configuración Fluent API y el snapshot exigen `UploadType`, `TemplateName`, `RequiredColumns`, `AllowedFormat`, `TemplateVersion`, `IsActive` y `CreatedAt`; además restringen `tipo_carga` a `X_FACTOR|X_MONTO`, `formato_permitido` a `CSV|XLSX|CSV/XLSX`, longitud 40 para tipo, 150 para nombre, 80 para formato y 30 para versión, con índice único por tipo-versión.
- La corrección reemplazó la inicialización inválida por una plantilla X Factor válida con `TemplateName`, `RequiredColumns`, `AllowedFormat` y `TemplateVersion`, sin modificar entidad, Fluent API, migraciones, snapshot ni modelo físico.
- Se conserva la prueba de solo lectura: las consultas `GET /api/backup-metadata` y `GET /api/backup-metadata/{id}` no modifican `Respaldo`, `Auditoria`, calificaciones tributarias, cargas masivas ni usuarios.
- Se conservan las pruebas de autorización, listado paginado, orden por defecto, filtros, parámetros inválidos, detalle, `404` y exclusión de rutas, observaciones y campos sensibles.
