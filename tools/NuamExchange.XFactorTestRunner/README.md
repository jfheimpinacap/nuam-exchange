# NuamExchange X Factor Test Runner

`NuamExchange.XFactorTestRunner` es una herramienta de consola .NET 8 para preparar futuras pruebas locales controladas de la Carga Masiva X Factor y generar evidencia técnica para el informe final del proyecto.

## Alcance

- No forma parte de la aplicación productiva.
- No está agregado a la solución backend `NuamExchange.sln`.
- Compila de forma independiente y no referencia proyectos del backend, frontend ni tests existentes.
- Debe usarse únicamente contra una API local.
- La etapa C001 no ejecuta pruebas reales, no realiza llamadas HTTP y no modifica datos.

## Uso de preflight

```bash
dotnet run --project ./tools/NuamExchange.XFactorTestRunner/NuamExchange.XFactorTestRunner.csproj -- preflight --api-base-url http://localhost:5000 --record-id 123
```

Argumentos disponibles:

- `--api-base-url <url>`: obligatorio. Debe ser una URL absoluta HTTP o HTTPS con host local permitido: `localhost`, `127.0.0.1` o `::1`.
- `--record-id <int>`: obligatorio. Debe ser un entero mayor que cero.
- `--output-dir <path>`: opcional. Debe estar fuera del repositorio actual. Si se omite, se resuelve a la carpeta de documentos del usuario en `NuamExchangeTestRuns`.
- `--help`: muestra ayuda y ejemplos.

## Seguridad

El runner rechaza dominios, IPs privadas remotas e IPs públicas. También rechaza directorios de evidencias ubicados dentro del repositorio para evitar generar archivos de salida junto al código fuente.

## Siguientes etapas previstas

Las etapas futuras podrán incorporar autenticación segura, ejecución de casos controlados, restauración de estado y generación de reportes técnicos. Estas capacidades no están implementadas en C001.
