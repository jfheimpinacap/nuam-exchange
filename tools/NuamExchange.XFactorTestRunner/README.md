# NuamExchange X Factor Test Runner

`NuamExchange.XFactorTestRunner` es una herramienta de consola .NET 8 aislada para preparar y ejecutar pruebas locales controladas de la Carga Masiva X Factor contra una API local autorizada.

## Alcance

- No forma parte de la aplicación productiva.
- No está agregado a `backend-dotnet/NuamExchange.sln`.
- Compila de forma independiente y no referencia backend, frontend, tests existentes ni base de datos.
- Debe usarse únicamente contra una API local (`localhost`, `127.0.0.1` o `::1`).
- `preflight` no ejecuta llamadas HTTP, no solicita credenciales y no modifica datos.
- `inspect` autentica, valida autorización y consulta usuario/registro sin modificar datos.
- `run` ejecuta XF-01 a XF-09, modifica temporalmente solo `AppliedFactor` del registro confirmado mediante `POST /api/tax-classifications/bulk-loads/x-factor`, consulta trazabilidad y restaura el factor original.
- Codex Cloud no debe ejecutar `run`, `inspect`, `preflight` contra APIs reales, cargas CSV, frontend ni base de datos durante validaciones automatizadas.

## Uso de preflight

```bash
dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- preflight --api-base-url http://localhost:5000 --record-id 123
```

## Uso de inspect

Configure credenciales mediante variables de entorno. No pase correo ni contraseña como argumentos.

```bash
export NUAM_XFACTOR_TEST_EMAIL="<configurado fuera del repositorio>"
export NUAM_XFACTOR_TEST_PASSWORD="<configurado fuera del repositorio>"

dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- inspect --api-base-url http://localhost:5000 --record-id 123
```

`inspect` realiza únicamente:

1. `POST /api/auth/login` para obtener un token solo en memoria.
2. `GET /api/auth/me` para confirmar rol `Administrador` o `Analista Tributario`.
3. `GET /api/tax-classifications/{recordId}` para capturar la línea base segura.

## Uso de run

`run` requiere confirmación explícita con `--confirm-write`. Sin esa bandera se detiene antes de login, antes de crear CSV y antes de cualquier llamada HTTP.

```bash
export NUAM_XFACTOR_TEST_EMAIL="<usuario-autorizado-local>"
export NUAM_XFACTOR_TEST_PASSWORD="<password-local>"

dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- \
  run \
  --api-base-url http://localhost:5000 \
  --record-id 1 \
  --expected-market BOLSA \
  --expected-instrument-code NUEX-PRUEBA-202607020001 \
  --expected-tax-period 2026 \
  --confirm-write
```

`run` valida antes de escribir que el registro consultado por `GET /api/tax-classifications/{id}` coincida exactamente con `--expected-market`, `--expected-instrument-code` y `--expected-tax-period`, y exige que `appliedFactor` inicial exista y sea decimal válido. Si falla la identidad o el factor inicial, no ejecuta cargas.

## Argumentos disponibles

- `--api-base-url <url>`: obligatorio. URL absoluta HTTP/HTTPS con host local permitido: `localhost`, `127.0.0.1` o `::1`.
- `--record-id <int>`: obligatorio. Entero mayor que cero.
- `--output-dir <path>`: opcional. Debe estar fuera del repositorio actual.
- `--expected-market <texto>`: obligatorio para `run`. Mercado esperado exacto del registro.
- `--expected-instrument-code <texto>`: obligatorio para `run`. Código de instrumento esperado exacto.
- `--expected-tax-period <int>`: obligatorio para `run`. Período tributario esperado exacto.
- `--confirm-write`: obligatorio para `run`. Confirma que el operador autoriza la modificación temporal de `AppliedFactor` del registro confirmado.
- `--help`: muestra ayuda y ejemplos.

## Evidencias externas

Si se omite `--output-dir`, las evidencias se crean fuera del repositorio en `NuamExchangeTestRuns` bajo el escritorio del usuario resuelto con `Environment.SpecialFolder.DesktopDirectory`:

- Windows: `%USERPROFILE%\Desktop\NuamExchangeTestRuns`
- Otros sistemas: escritorio del usuario si existe; si no existe o no es seguro, una alternativa fuera del repositorio.

El runner rechaza explícitamente cualquier ruta dentro del repositorio o sus subcarpetas.

Cada ejecución de `run` crea una carpeta única similar a `YYYYMMDD-HHmmss-x-factor-run-record-{recordId}` con:

- `run-summary.md`
- `results.json`
- `execution.log`
- `baseline-tax-classification.json`
- `post-run-tax-classification.json`
- `authenticated-user.json`
- `history-before.json`
- `history-after.json`
- `test-matrix.csv`
- `restoration-result.json`
- `csv/`
- `responses/`

Las evidencias no contienen contraseña, token JWT, encabezados `Authorization`, correo, cuerpo de login ni claims completos.

## Casos cubiertos por run

- XF-01: archivo válido.
- XF-02: falta el campo multipart `file`.
- XF-03: extensión incorrecta.
- XF-04: encabezado incorrecto.
- XF-05: factor inválido (`INVALID_APPLIED_FACTOR`).
- XF-06: registro inexistente (`NOT_FOUND`).
- XF-07: identidad duplicada (`DUPLICATE_ROW`).
- XF-08: archivo mixto con una fila válida y dos fallidas.
- XF-09: solicitud sin token (`401`).
- XF-10: `NOT_EXECUTED`; Supervisor sin permiso queda pendiente de cuenta Supervisor local autorizada y está cubierto por pruebas xUnit existentes.

## Restauración automática

La restauración se ejecuta en `finally` si pudo existir una modificación real del factor. El runner genera `RESTORE-factor-original.csv`, lo carga por la misma ruta autorizada de X Factor, consulta nuevamente el registro y confirma que `AppliedFactor` volvió al baseline y que los demás campos de negocio permanecen iguales. Si la restauración falla, el resultado global falla y se muestra una advertencia con el directorio de evidencias y el factor baseline pendiente.

## Seguridad

El cliente HTTP desactiva redirecciones automáticas. El runner solo acepta credenciales desde `NUAM_XFACTOR_TEST_EMAIL` y `NUAM_XFACTOR_TEST_PASSWORD`; no acepta correo, contraseña ni token por argumentos. No crea ni elimina calificaciones y no llama rutas de creación, edición, copia, validación supervisora ni X Amount.
