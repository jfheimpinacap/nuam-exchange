# Prompt 041 — Importación curada del frontend React

- Fecha de importación: 2026-06-29.
- Repositorio fuente: `Rodbrok/Nuam-Exchange`.
- Rama fuente: `main`.
- SHA fuente inspeccionado: no disponible desde `git clone` por bloqueo HTTP 403 del proxy; se inspeccionó la rama pública `main` mediante la vista web de GitHub, incluyendo estructura `src/`, metadatos de dependencias y README visible.
- Decisión: importación curada de la interfaz React dentro de `frontend/`, sin `subtree`, `submodule`, merge de historiales ni remote externo.

## Rutas importadas y adaptadas

- `frontend/src/**`: se reemplazó la aplicación mínima por una interfaz administrativa curada con carpetas `api`, `app`, `components`, `features`, `layouts`, `mocks`, `pages`, `routes`, `styles`, `types` y `utils`.
- `frontend/index.html`.
- `frontend/package.json` y `frontend/package-lock.json`.
- `frontend/tsconfig.json`, `frontend/tsconfig.app.json` y `frontend/tsconfig.node.json`.
- `frontend/eslint.config.js`.
- `frontend/.env.example`.
- `frontend/src/vite-env.d.ts`.

## Rutas excluidas

No se incorporaron desde el repositorio externo: `backend/`, `.github/`, `docs/api/`, `global.json`, README externo, `.gitignore` externo, `node_modules/`, `dist/`, `coverage/`, archivos `.env` reales, migraciones, seeds, scripts backend, workflows ni artefactos generados.

## Razón para no usar subtree

No se usó `git subtree` porque el objetivo aprobado fue una importación curada dentro del monorepo principal, sin arrastrar historial externo, backend externo, CI externo ni contratos provisionales. El backend principal continúa siendo la fuente de verdad para rutas, DTOs, políticas, roles y autorización.

## Preservación de Vite del repositorio principal

Se conservó `frontend/vite.config.ts` del repositorio principal con puerto `5173`, proxy local para `/api`, proxy local para `/health`, destino `http://localhost:5000` y salida de build principal hacia `../backend-dotnet/src/NuamExchange.Api/wwwroot`. La validación de este prompt usó un `outDir` temporal para no escribir en el backend.

## Estado funcional temporal

El frontend queda temporalmente en modo mock (`VITE_DATA_SOURCE=mock`). La capa API preparada conserva `VITE_API_BASE_URL=/api/v1`, pero la normalización e integración real con backend comienzan recién desde Prompt 042. No se conectaron login real, JWT, endpoints reales, formularios reales ni módulos backend.

## Validaciones ejecutadas

- `npm ci`: exitoso.
- `npm run lint`: exitoso con advertencias Fast Refresh no bloqueantes.
- `./node_modules/.bin/tsc -b --pretty false`: exitoso.
- `./node_modules/.bin/vite build --outDir .tmp-dist-prompt041 --emptyOutDir`: exitoso.
- `rm -rf .tmp-dist-prompt041`: ejecutado; el directorio temporal fue eliminado.

## Seguridad y alcance

No se importaron backend externo, CI externo, `global.json`, contratos API provisionales, `node_modules`, `.env` real ni secretos. No se modificaron archivos bajo `backend-dotnet/`, `.github/` ni `docs/api/`.
