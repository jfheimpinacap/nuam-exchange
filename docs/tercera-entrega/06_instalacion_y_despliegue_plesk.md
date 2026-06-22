# 06. Instalación y despliegue Plesk

Prompt 006 no modificó Plesk, Wirenet ni el subdominio público. SQL Server productivo aún no ha sido configurado.

La futura cadena de conexión productiva `ConnectionStrings:NuamTributariaDb` deberá configurarse fuera del repositorio mediante variables de entorno, configuración segura del hosting o mecanismo equivalente. No deben versionarse usuarios, contraseñas, IPs reales ni certificados.

La creación de base, migraciones y despliegue productivo se abordarán en prompts posteriores.

## Credenciales productivas SQL Server

Las credenciales productivas de SQL Server se configurarán únicamente en Plesk o en una configuración segura equivalente del servidor. Nunca deben quedar en `appsettings.json`, archivos de ejemplo, commits ni documentación pública.

La instancia SQL Server productiva no ha sido utilizada aún. Prompt 007 solo prepara el flujo local de migraciones y no toca Plesk, Wirenet, subdominios, hosting ni producción.
