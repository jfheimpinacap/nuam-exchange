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
