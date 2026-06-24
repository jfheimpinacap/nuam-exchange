# Seguridad, autenticación y roles

## Implementado en Prompt 009

- Hash seguro de contraseñas con BCrypt para creación y validación de credenciales.
- Autenticación JWT con tokens Bearer.
- Expiración configurable mediante la sección `Jwt:AccessTokenMinutes`.
- Validación de issuer, audience, firma y expiración, con `ClockSkew` explícito en cero.
- Claims mínimos de usuario y rol, sin incluir hash de contraseña ni permisos detallados dentro del token.
- Roles base: `Administrador`, `Analista Tributario` y `Supervisor`.
- Permisos base y relaciones rol-permiso preparados mediante seed idempotente ejecutado solo desde bootstrap de Development.
- Auditoría de login exitoso y fallido en la tabla `Auditoria`, sin registrar contraseñas ni hashes.
- Endpoint de bootstrap exclusivo de Development para crear el primer administrador local cuando no existen usuarios.
- Comportamiento seguro si JWT no está configurado: la API inicia, `/health` continúa disponible y los endpoints de autenticación no emiten tokens.

## Pendiente

- Control UI por permisos.
- Bloqueo temporal por intentos fallidos.
- Gestión administrativa completa de usuarios.
- Auditoría de operaciones de negocio.
- Seguridad de cargas de archivos.

## Administración segura de usuarios

Se agregó el contrato backend para administración de usuarios bajo `/api/admin`, protegido completamente por la política `AdministratorOnly` y el rol `Administrador`. Esta protección mantiene el enfoque validado: el JWT identifica al usuario y su rol, mientras que la autorización dinámica basada en permisos queda pendiente para una etapa posterior.

La política de contraseñas fue extraída para reutilizarse en bootstrap, creación administrativa de usuarios y restablecimiento de contraseña. El almacenamiento continúa usando BCrypt y no se retornan ni registran passwords, hashes ni tokens.

La administración permite gestionar el estado activo/inactivo sin borrado físico, restablecer contraseñas de forma segura y consultar roles/permisos disponibles. Además, se incorporan protecciones para impedir que el Administrador autenticado se desactive o cambie su propio rol, y para evitar desactivar o degradar al último Administrador activo.

Pendiente: autorización dinámica basada en permisos persistidos en base de datos.

## Prompt 012 — Roles personalizados y permisos controlados

Se incorporó la administración controlada de roles personalizados bajo `/api/admin`, protegida por la política `AdministratorOnly`.

### Roles base protegidos

Los roles `Administrador`, `Analista Tributario` y `Supervisor` permanecen como roles base oficiales. No pueden ser renombrados, desactivados ni recibir cambios de permisos mediante la nueva administración de roles.

### Administración personalizada

Los administradores pueden crear y actualizar roles personalizados, siempre que el nombre sea único, no reservado y cumpla las reglas de longitud. La desactivación se bloquea cuando existen usuarios activos asignados al rol.

### Asignación controlada de permisos

La asignación de permisos reemplaza de forma completa las relaciones del rol personalizado usando solo permisos existentes. No se crean permisos nuevos ni se modifica el catálogo.

### Auditoría

Las acciones exitosas sobre roles registran auditoría con `ROLE_CREATED`, `ROLE_UPDATED` y `ROLE_PERMISSIONS_UPDATED`, sin almacenar tokens, contraseñas, hashes ni secretos.

### Pendiente

Queda pendiente implementar autorización dinámica basada directamente en permisos efectivos.

## Política de escritura de Calificaciones Tributarias

- `TaxClassificationWrite` permite crear calificaciones tributarias solo a `Administrador` y `Analista Tributario`.
- `Supervisor` conserva acceso de solo lectura mediante `TaxClassificationRead` y no puede ejecutar `POST /api/tax-classifications`.
- La creación usa el usuario autenticado del JWT como actor de auditoría y como `CreatorUserId` de la calificación creada.

## Prompt 017 — Edición e historial de calificaciones tributarias

- `Administrador` y `Analista Tributario` pueden editar calificaciones tributarias mediante `PUT /api/tax-classifications/{id}` con la política `TaxClassificationWrite`.
- `Supervisor` conserva acceso de lectura mediante `TaxClassificationRead`, incluido `GET /api/tax-classifications/{id}/history`.
- `Supervisor` no puede editar calificaciones tributarias y debe recibir `403 Forbidden` en el endpoint `PUT`.

## Copia de calificaciones tributarias

- `Administrador` y `Analista Tributario` pueden copiar calificaciones tributarias mediante `POST /api/tax-classifications/{id}/copy` con la política `TaxClassificationWrite`.
- `Supervisor` conserva permisos de lectura e historial mediante `TaxClassificationRead`, pero no puede copiar calificaciones tributarias porque no pertenece a `TaxClassificationWrite`.

## Matriz de permisos para calificaciones tributarias con validación supervisora

| Rol | Lectura | Historial | Creación | Edición | Copia | Validación supervisora |
| --- | --- | --- | --- | --- | --- | --- |
| Administrador | Sí (`TaxClassificationRead`) | Sí (`TaxClassificationRead`) | Sí (`TaxClassificationWrite`) | Sí (`TaxClassificationWrite`) | Sí (`TaxClassificationWrite`) | Sí (`TaxClassificationSupervise`) |
| Analista Tributario | Sí (`TaxClassificationRead`) | Sí (`TaxClassificationRead`) | Sí (`TaxClassificationWrite`) | Sí (`TaxClassificationWrite`) | Sí (`TaxClassificationWrite`) | No (`403 Forbidden`) |
| Supervisor | Sí (`TaxClassificationRead`) | Sí (`TaxClassificationRead`) | No (`403 Forbidden`) | No (`403 Forbidden`) | No (`403 Forbidden`) | Sí (`TaxClassificationSupervise`) |

`TaxClassificationSupervise` reutiliza claims de rol del JWT y permite solo `Administrador` y `Supervisor`. No modifica login, bootstrap, entidades de seguridad ni datos de roles/permisos.

## Carga Masiva X Factor

- `POST /api/tax-classifications/bulk-loads/x-factor` usa la política `TaxClassificationWrite`.
- `Administrador` y `Analista Tributario` pueden ejecutar la carga masiva X Factor.
- `Supervisor` no puede ejecutarla y recibe `403 Forbidden` cuando está autenticado.
- La carga no modifica `TaxClassificationRead` ni `TaxClassificationSupervise`; el Supervisor conserva lectura, historial y validación supervisora, pero no escritura ni cargas masivas tributarias.
