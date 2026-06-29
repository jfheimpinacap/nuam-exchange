# Prompt 041 / 042 — Importación curada del frontend React

- Fecha de importación inicial: 2026-06-29.
- Corrección Prompt 042: 2026-06-29.
- Repositorio fuente: `Rodbrok/Nuam-Exchange`.
- Rama fuente: `main`.
- Commit fuente obligatorio: `9f86c36636bd9a73dee3ac16901129c11db77bde`.
- Obtención: se inspeccionó contenido público del commit fuente desde GitHub web/raw porque el `git clone` directo desde shell quedó bloqueado por proxy HTTP 403.
- Decisión: importación curada dentro de `frontend/`, sin `subtree`, `submodule`, merge de historiales ni remote externo.

## Corrección aplicada por Prompt 042

Prompt 042 corrige la importación simplificada inicial y sustituye la navegación manual por una estructura compatible con la arquitectura del frontend fuente: `BrowserRouter`, Login demostrativo, `SessionProvider` en memoria, `ProtectedRoute`, layout administrativo, rutas URL reales, módulos visuales completos y capa API tipada con modo mock predeterminado.

## Árbol frontend importado/adaptado

- `frontend/src/api/client/`.
- `frontend/src/api/config/`.
- `frontend/src/api/context/`.
- `frontend/src/api/contracts/`.
- `frontend/src/api/hooks/`.
- `frontend/src/api/mappers/`.
- `frontend/src/api/services/`.
- `frontend/src/app/session/`.
- `frontend/src/components/`.
- `frontend/src/features/`.
- `frontend/src/layouts/`.
- `frontend/src/mocks/`.
- `frontend/src/pages/`.
- `frontend/src/routes/`.
- `frontend/src/styles/`.
- `frontend/src/types/`.
- `frontend/src/utils/`.
- `frontend/src/main.tsx` y `frontend/src/vite-env.d.ts`.
- `frontend/index.html`, `frontend/package.json`, `frontend/package-lock.json`, TypeScript, ESLint y `.env.example`.

## Rutas reales conservadas

El frontend conserva rutas URL para `/login`, `/inicio`, `/calificaciones`, `/calificaciones/nueva`, `/calificaciones/:id/editar`, `/calificaciones/:id/copiar`, `/cargas/x-factor`, `/cargas/x-monto`, `/plantillas-carga`, `/reportes`, `/administracion/usuarios`, `/administracion/roles-permisos`, `/auditoria` y `/respaldos`.

## Exclusiones

No se incorporaron desde el repositorio externo: `backend/`, `.github/`, `docs/api/`, `global.json`, README externo, `.gitignore` externo, `node_modules/`, `dist/`, `coverage/`, archivos `.env` reales, migraciones, seeds, scripts backend, workflows ni artefactos generados.

## Preservación de Vite

Se conserva `frontend/vite.config.ts` del repositorio principal con puerto `5173`, proxy `/api`, proxy `/health`, destino `http://localhost:5000` y salida principal a `../backend-dotnet/src/NuamExchange.Api/wwwroot`. La validación usó `--outDir .tmp-dist-prompt042` para no escribir en backend.

## Estado funcional temporal

El frontend continúa en modo mock (`VITE_DATA_SOURCE=mock`) con `VITE_API_BASE_URL=/api/v1` y `VITE_API_TIMEOUT_MS=10000`. No se conectó API real, JWT, credenciales, backend, persistencia ni endpoints reales.

## Validaciones ejecutadas

- `npm ci`: exitoso.
- `npm run lint`: exitoso con advertencias Fast Refresh no bloqueantes.
- `./node_modules/.bin/tsc -b --pretty false`: exitoso.
- `./node_modules/.bin/vite build --outDir .tmp-dist-prompt042 --emptyOutDir`: exitoso.
- `rm -rf .tmp-dist-prompt042`: ejecutado; directorio temporal eliminado.

## Seguridad y alcance

No hubo cambios bajo `backend-dotnet/`, `.github/` ni `docs/api/`. No se importaron backend externo, CI externo, `global.json`, contratos API provisionales, `node_modules`, `.env` real ni secretos.
