# Contrato API — Roles y permisos

## Objetivo

Este módulo permite administrar roles personalizados y asignarles permisos existentes del catálogo tributario sin modificar el modelo de datos ni crear permisos nuevos.

## Autorización requerida

Todos los endpoints requieren JWT válido y política `AdministratorOnly`. Un usuario sin token recibe `401`; un usuario autenticado sin rol Administrador recibe `403`.

## Roles base protegidos

Los roles `Administrador`, `Analista Tributario` y `Supervisor` son roles base del sistema. No pueden crearse duplicados con esos nombres, renombrarse, desactivarse ni recibir cambios de permisos mediante estos endpoints. Todo intento de modificar su configuración responde `409` con un mensaje seguro.

## Endpoints

### GET `/api/admin/roles`

Lista roles activos e inactivos con permisos ordenados por código.

### GET `/api/admin/roles/{id}`

Retorna el detalle de un rol.

```json
{
  "id": 2,
  "name": "Analista Tributario",
  "description": "Rol base del sistema",
  "isActive": true,
  "isSystemRole": true,
  "permissions": [
    { "id": 1, "code": "reports.read", "description": "Consulta de reportes" }
  ]
}
```

### POST `/api/admin/roles`

Crea un rol personalizado activo con permisos existentes.

```json
{
  "name": "Revisor de Cargas",
  "description": "Revisa cargas tributarias antes de su validación.",
  "permissionIds": [1, 2, 3]
}
```

Responde `201` con el detalle del rol creado.

### PUT `/api/admin/roles/{id}`

Actualiza nombre, descripción y estado de un rol personalizado. No modifica permisos.

```json
{
  "name": "Revisor de Cargas",
  "description": "Descripción actualizada.",
  "isActive": true
}
```

No se permite desactivar un rol con usuarios activos asignados; en ese caso responde `409`.

### PUT `/api/admin/roles/{id}/permissions`

Reemplaza los permisos de un rol personalizado usando únicamente permisos existentes.

```json
{
  "permissionIds": [1, 4, 7]
}
```

Responde con el detalle actualizado del rol y permisos ordenados por código.

### GET `/api/admin/permissions`

Consulta de solo lectura del catálogo de permisos, ordenado por código. Este prompt no crea, elimina ni modifica permisos.

## Códigos HTTP

- `200`: consulta o actualización exitosa.
- `201`: rol personalizado creado.
- `400`: request inválido, permisos inexistentes o repetidos.
- `401`: JWT ausente o inválido.
- `403`: usuario autenticado sin rol Administrador.
- `404`: rol inexistente.
- `409`: nombre duplicado, nombre reservado, rol base protegido o rol con usuarios activos.
- `503`: base de datos no disponible, sin detalles internos.

## Validaciones

- `name` obligatorio, trimmeado, máximo 80 caracteres y único ignorando mayúsculas/minúsculas.
- `name` no puede ser `Administrador`, `Analista Tributario` ni `Supervisor`.
- `description` opcional y máximo 250 caracteres.
- `permissionIds` obligatorio, con al menos un permiso, sin duplicados y con IDs existentes.

## Política de no borrado físico

No existe endpoint de eliminación. La desactivación lógica solo aplica a roles personalizados sin usuarios activos.

## Reflejo en permisos efectivos

Los permisos no se incluyen en el JWT. Los cambios de permisos por rol se reflejan en futuras consultas a `GET /api/auth/permissions` sin emitir un token nuevo.
