# Contrato API: administración de usuarios

Fecha: 2026-06-23.

## Objetivo

El módulo backend de administración de usuarios permite que un usuario con rol **Administrador** consulte, cree, actualice, active/inactive y restablezca contraseñas de usuarios del sistema. También expone catálogos de roles y permisos para una futura interfaz administrativa.

No incorpora frontend, no modifica entidades, no crea migraciones y no altera roles ni permisos existentes.

## Seguridad y rol requerido

Todos los endpoints se publican bajo `/api/admin` y requieren JWT válido con la política `AdministratorOnly`, basada en el rol `Administrador`.

Respuestas esperadas de seguridad:

- `401 Unauthorized`: no existe token válido.
- `403 Forbidden`: el token es válido, pero el rol no es Administrador.
- `503 Service Unavailable`: la base de datos no está configurada o no está disponible, sin exponer detalles técnicos.

## Política de contraseñas

Las contraseñas usadas en bootstrap, creación administrativa y restablecimiento deben cumplir:

- mínimo 12 caracteres;
- al menos una mayúscula;
- al menos una minúscula;
- al menos un número;
- al menos un símbolo.

Las contraseñas se almacenan únicamente como hash BCrypt. La API nunca retorna password ni passwordHash y Auditoria no registra contraseñas, hashes ni tokens.

## Endpoints

### 1. `GET /api/admin/users`

Lista usuarios administrables.

Query opcional: `search`, `roleId`, `isActive`, `page` por defecto `1`, `pageSize` por defecto `20` y máximo `100`.

Respuesta `200 OK`:

```json
{
  "items": [
    {
      "id": 1,
      "fullName": "Nombre Apellido",
      "email": "correo@ejemplo.cl",
      "jobTitle": "Cargo",
      "role": { "id": 1, "name": "Administrador" },
      "isActive": true,
      "lastAccessAt": "2026-06-23T00:00:00Z",
      "createdAt": "2026-06-23T00:00:00Z",
      "updatedAt": "2026-06-23T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

### 2. `GET /api/admin/users/{id}`

Consulta un usuario específico con los mismos campos seguros del listado.

- `200 OK`: usuario encontrado.
- `404 Not Found`: usuario inexistente.

### 3. `POST /api/admin/users`

Crea un usuario activo por defecto.

Request:

```json
{
  "fullName": "Nombre del usuario",
  "email": "usuario@ejemplo.cl",
  "password": "PasswordSeguro!123",
  "jobTitle": "Analista Tributario",
  "roleId": 2
}
```

Respuestas:

- `201 Created`: usuario creado.
- `400 Bad Request`: datos inválidos, rol inexistente/inactivo o password inseguro.
- `409 Conflict`: correo ya registrado.

Registra Auditoria `USER_CREATED` sobre entidad `Usuario`.

### 4. `PUT /api/admin/users/{id}`

Actualiza nombre, correo, cargo, rol y estado activo/inactivo. No modifica contraseñas.

Request:

```json
{
  "fullName": "Nombre actualizado",
  "email": "usuario@ejemplo.cl",
  "jobTitle": "Cargo actualizado",
  "roleId": 2,
  "isActive": true
}
```

Respuestas:

- `200 OK`: usuario actualizado o sin cambios materiales.
- `400 Bad Request`: datos inválidos o rol inexistente/inactivo.
- `404 Not Found`: usuario inexistente.
- `409 Conflict`: correo duplicado o infracción de protecciones administrativas.

Registra Auditoria `USER_UPDATED` cuando existen cambios exitosos.

### 5. `POST /api/admin/users/{id}/reset-password`

Restablece la contraseña sin crear tokens ni enviar correos.

Request:

```json
{ "newPassword": "NuevaPassword!123" }
```

Respuestas:

- `204 No Content`: contraseña restablecida.
- `400 Bad Request`: password inseguro.
- `404 Not Found`: usuario inexistente.

Registra Auditoria `USER_PASSWORD_RESET`.

### 6. `GET /api/admin/roles`

Entrega roles disponibles ordenados por nombre, con permisos únicos ordenados alfabéticamente.

Respuesta `200 OK`:

```json
[
  {
    "id": 1,
    "name": "Administrador",
    "description": "Descripción existente",
    "isActive": true,
    "permissions": ["audit.read", "users.manage"]
  }
]
```

### 7. `GET /api/admin/permissions`

Entrega permisos disponibles ordenados por código.

Respuesta `200 OK`:

```json
[
  { "id": 1, "code": "users.manage", "description": "Descripción existente" }
]
```

## Protección del último Administrador

El backend impide:

- desactivar el propio usuario autenticado;
- modificar el rol del propio usuario autenticado;
- desactivar al último Administrador activo;
- quitar el rol Administrador al último Administrador activo.

Estas infracciones responden `409 Conflict` con mensaje seguro en español.

## Sin borrado físico

No existe endpoint de eliminación física. La baja administrativa se realiza con `isActive: false`.

## Pendiente

La modificación de roles y permisos, junto con autorización dinámica basada en permisos persistidos, se abordará en una etapa posterior.
