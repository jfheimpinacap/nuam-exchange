# NuamExchange X Factor Test Runner

`NuamExchange.XFactorTestRunner` es una herramienta de consola .NET 8 para preparar futuras pruebas locales controladas de la Carga Masiva X Factor y generar evidencia técnica para el informe final del proyecto.

## Alcance

- No forma parte de la aplicación productiva.
- No está agregado a la solución backend `NuamExchange.sln`.
- Compila de forma independiente y no referencia proyectos del backend, frontend ni tests existentes.
- Debe usarse únicamente contra una API local (`localhost`, `127.0.0.1` o `::1`).
- `preflight` no ejecuta pruebas reales, no realiza llamadas HTTP y no modifica datos.
- `inspect` en C002 solo consulta datos mínimos, valida autorización y genera evidencia externa; no ejecuta cargas CSV ni operaciones de modificación.

## Uso de preflight

```bash
dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- preflight --api-base-url http://localhost:5000 --record-id 123
```

## Uso de inspect

Antes de ejecutar, configure las credenciales en variables de entorno. No pase correo ni contraseña como argumentos de consola.

```bash
export NUAM_XFACTOR_TEST_EMAIL="<configurado fuera del repositorio>"
export NUAM_XFACTOR_TEST_PASSWORD="<configurado fuera del repositorio>"

dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- inspect --api-base-url http://localhost:5000 --record-id 123
```

`inspect` realiza únicamente estas solicitudes contra la API local validada:

1. `POST /api/auth/login` para obtener un token en memoria.
2. `GET /api/auth/me` para confirmar que el rol sea `Administrador` o `Analista Tributario`.
3. `GET /api/tax-classifications/{recordId}` para capturar la línea base segura del registro.

No llama rutas de carga masiva, creación, edición, copia ni validación supervisora.

## Argumentos disponibles

- `--api-base-url <url>`: obligatorio. Debe ser una URL absoluta HTTP o HTTPS con host local permitido: `localhost`, `127.0.0.1` o `::1`.
- `--record-id <int>`: obligatorio. Debe ser un entero mayor que cero.
- `--output-dir <path>`: opcional. Debe estar fuera del repositorio actual. Si se omite, se resuelve a la carpeta de documentos del usuario en `NuamExchangeTestRuns`.
- `--help`: muestra ayuda y ejemplos.

## Evidencias externas

Cada ejecución de `inspect` crea una carpeta única fuera del repositorio con formato similar a `YYYYMMDD-HHmmss-x-factor-inspect-record-{recordId}`. Dentro se generan:

- `run-summary.md`
- `results.json`
- `execution.log`
- `baseline-tax-classification.json`
- `authenticated-user.json`

Las evidencias no contienen contraseña, token JWT, encabezados `Authorization`, correo ni cuerpos de login. El token permanece solo en memoria durante la ejecución.

## Seguridad

El runner rechaza dominios, IPs privadas remotas e IPs públicas. También rechaza directorios de evidencias ubicados dentro del repositorio para evitar generar archivos de salida junto al código fuente. El cliente HTTP de `inspect` desactiva redirecciones automáticas y falla ante cualquier respuesta de redirección.
