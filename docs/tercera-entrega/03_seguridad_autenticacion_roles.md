# 03. Seguridad, autenticación y roles

Prompt 006 dejó modeladas de forma persistente las entidades `Role`, `Permission`, `RolePermission` y `ApplicationUser`, junto con sus tablas `Rol`, `Permiso`, `RolPermiso` y `Usuario`. Esto prepara la base para roles y permisos, pero no implementa autenticación funcional.

Aún están pendientes password hashing real, flujos de inicio de sesión, emisión y validación de JWT, políticas de autorización, control de acceso por endpoint y administración funcional de permisos.

`AuditLog` ya existe como entidad persistente y tabla `Auditoria`, pero todavía no registra eventos reales desde la aplicación. La auditoría funcional se implementará en una etapa posterior.
