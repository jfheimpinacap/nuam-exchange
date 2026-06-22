using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuamExchange.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permiso",
                columns: table => new
                {
                    permiso_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permiso", x => x.permiso_id);
                });

            migrationBuilder.CreateTable(
                name: "PlantillaCarga",
                columns: table => new
                {
                    plantilla_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo_carga = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    nombre_plantilla = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    columnas_requeridas = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    formato_permitido = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    version_plantilla = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "1.0"),
                    activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaCarga", x => x.plantilla_id);
                    table.CheckConstraint("CK_PlantillaCarga_FormatoPermitido", "[formato_permitido] IN ('CSV','XLSX','CSV/XLSX')");
                    table.CheckConstraint("CK_PlantillaCarga_TipoCarga", "[tipo_carga] IN ('X_FACTOR','X_MONTO')");
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "RolPermiso",
                columns: table => new
                {
                    rol_permiso_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    permiso_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermiso", x => x.rol_permiso_id);
                    table.ForeignKey(
                        name: "FK_RolPermiso_Permiso_permiso_id",
                        column: x => x.permiso_id,
                        principalTable: "Permiso",
                        principalColumn: "permiso_id");
                    table.ForeignKey(
                        name: "FK_RolPermiso_Rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "Rol",
                        principalColumn: "rol_id");
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rol_id = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cargo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ultimo_acceso_en = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    creado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    actualizado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "Rol",
                        principalColumn: "rol_id");
                });

            migrationBuilder.CreateTable(
                name: "ArchivoCarga",
                columns: table => new
                {
                    archivo_carga_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    plantilla_id = table.Column<int>(type: "int", nullable: false),
                    tipo_carga = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    nombre_archivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ruta_archivo = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    hash_archivo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    tamanio_bytes = table.Column<long>(type: "bigint", nullable: true),
                    estado_carga = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "RECIBIDO"),
                    total_registros = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    registros_validos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    registros_con_error = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    observacion = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    fecha_carga = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivoCarga", x => x.archivo_carga_id);
                    table.CheckConstraint("CK_ArchivoCarga_Contadores", "[total_registros] >= 0 AND [registros_validos] >= 0 AND [registros_con_error] >= 0");
                    table.CheckConstraint("CK_ArchivoCarga_EstadoCarga", "[estado_carga] IN ('RECIBIDO','EN_VALIDACION','PROCESADO','PROCESADO_CON_ERRORES','OBSERVADO','RECHAZADO')");
                    table.CheckConstraint("CK_ArchivoCarga_Extension", "[extension] IN ('CSV','XLSX')");
                    table.CheckConstraint("CK_ArchivoCarga_Tamanio", "[tamanio_bytes] IS NULL OR [tamanio_bytes] > 0");
                    table.CheckConstraint("CK_ArchivoCarga_TipoCarga", "[tipo_carga] IN ('X_FACTOR','X_MONTO')");
                    table.ForeignKey(
                        name: "FK_ArchivoCarga_PlantillaCarga_plantilla_id",
                        column: x => x.plantilla_id,
                        principalTable: "PlantillaCarga",
                        principalColumn: "plantilla_id");
                    table.ForeignKey(
                        name: "FK_ArchivoCarga_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    auditoria_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    entidad_afectada = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    registro_afectado_id = table.Column<int>(type: "int", nullable: true),
                    accion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    detalle = table.Column<string>(type: "nvarchar(900)", maxLength: 900, nullable: true),
                    valor_anterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valor_nuevo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ip_origen = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    fecha_accion = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.auditoria_id);
                    table.ForeignKey(
                        name: "FK_Auditoria_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "CalificacionTributaria",
                columns: table => new
                {
                    calificacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_creador_id = table.Column<int>(type: "int", nullable: false),
                    mercado = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    codigo_instrumento = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    nombre_instrumento = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    tipo_calificacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    porcentaje_actualizacion = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    factor_aplicado = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    monto_referencia = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "CLP"),
                    periodo_tributario = table.Column<int>(type: "int", nullable: false),
                    vigente_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    vigente_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    estado_calificacion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "VIGENTE"),
                    creado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    actualizado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionTributaria", x => x.calificacion_id);
                    table.CheckConstraint("CK_CalificacionTributaria_Estado", "[estado_calificacion] IN ('BORRADOR','VIGENTE','OBSERVADA','REEMPLAZADA','ANULADA')");
                    table.CheckConstraint("CK_CalificacionTributaria_FactorAplicado", "[factor_aplicado] IS NULL OR [factor_aplicado] >= 0");
                    table.CheckConstraint("CK_CalificacionTributaria_MontoReferencia", "[monto_referencia] IS NULL OR [monto_referencia] >= 0");
                    table.CheckConstraint("CK_CalificacionTributaria_PeriodoTributario", "[periodo_tributario] BETWEEN 2000 AND 2100");
                    table.CheckConstraint("CK_CalificacionTributaria_PorcentajeActualizacion", "[porcentaje_actualizacion] IS NULL OR [porcentaje_actualizacion] >= 0");
                    table.CheckConstraint("CK_CalificacionTributaria_Vigencia", "[vigente_hasta] IS NULL OR [vigente_desde] <= [vigente_hasta]");
                    table.ForeignKey(
                        name: "FK_CalificacionTributaria_Usuario_usuario_creador_id",
                        column: x => x.usuario_creador_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "ReporteTributario",
                columns: table => new
                {
                    reporte_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo_reporte = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    filtros_aplicados = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    formato = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ruta_reporte = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    generado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReporteTributario", x => x.reporte_id);
                    table.CheckConstraint("CK_ReporteTributario_Formato", "[formato] IN ('PDF','XLSX','CSV')");
                    table.ForeignKey(
                        name: "FK_ReporteTributario_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "Respaldo",
                columns: table => new
                {
                    respaldo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    tipo_respaldo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ruta_respaldo = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    estado_respaldo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "PROGRAMADO"),
                    fecha_respaldo = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    observacion = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respaldo", x => x.respaldo_id);
                    table.CheckConstraint("CK_Respaldo_EstadoRespaldo", "[estado_respaldo] IN ('PROGRAMADO','EJECUTADO','FALLIDO','RESTAURADO')");
                    table.CheckConstraint("CK_Respaldo_TipoRespaldo", "[tipo_respaldo] IN ('BASE_DATOS','ARCHIVOS','COMPLETO')");
                    table.ForeignKey(
                        name: "FK_Respaldo_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "ErrorCargaMasiva",
                columns: table => new
                {
                    error_carga_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    archivo_carga_id = table.Column<int>(type: "int", nullable: false),
                    numero_fila = table.Column<int>(type: "int", nullable: true),
                    columna = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    descripcion_error = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: false),
                    severidad = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "ERROR"),
                    creado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorCargaMasiva", x => x.error_carga_id);
                    table.CheckConstraint("CK_ErrorCargaMasiva_NumeroFila", "[numero_fila] IS NULL OR [numero_fila] > 0");
                    table.CheckConstraint("CK_ErrorCargaMasiva_Severidad", "[severidad] IN ('ADVERTENCIA','ERROR','CRITICO')");
                    table.ForeignKey(
                        name: "FK_ErrorCargaMasiva_ArchivoCarga_archivo_carga_id",
                        column: x => x.archivo_carga_id,
                        principalTable: "ArchivoCarga",
                        principalColumn: "archivo_carga_id");
                });

            migrationBuilder.CreateTable(
                name: "DetalleCargaMasiva",
                columns: table => new
                {
                    detalle_carga_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    archivo_carga_id = table.Column<int>(type: "int", nullable: false),
                    calificacion_id = table.Column<int>(type: "int", nullable: true),
                    numero_fila = table.Column<int>(type: "int", nullable: false),
                    campo_afectado = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    valor_factor = table.Column<decimal>(type: "decimal(18,8)", nullable: true),
                    valor_monto = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    valor_texto_original = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    estado_fila = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "PENDIENTE"),
                    observacion = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    creado_en = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleCargaMasiva", x => x.detalle_carga_id);
                    table.CheckConstraint("CK_DetalleCargaMasiva_EstadoFila", "[estado_fila] IN ('PENDIENTE','VALIDA','CON_ERROR','APLICADA','IGNORADA')");
                    table.CheckConstraint("CK_DetalleCargaMasiva_NumeroFila", "[numero_fila] > 0");
                    table.CheckConstraint("CK_DetalleCargaMasiva_ValorFactor", "[valor_factor] IS NULL OR [valor_factor] >= 0");
                    table.CheckConstraint("CK_DetalleCargaMasiva_ValorMonto", "[valor_monto] IS NULL OR [valor_monto] >= 0");
                    table.ForeignKey(
                        name: "FK_DetalleCargaMasiva_ArchivoCarga_archivo_carga_id",
                        column: x => x.archivo_carga_id,
                        principalTable: "ArchivoCarga",
                        principalColumn: "archivo_carga_id");
                    table.ForeignKey(
                        name: "FK_DetalleCargaMasiva_CalificacionTributaria_calificacion_id",
                        column: x => x.calificacion_id,
                        principalTable: "CalificacionTributaria",
                        principalColumn: "calificacion_id");
                });

            migrationBuilder.CreateTable(
                name: "HistorialCalificacion",
                columns: table => new
                {
                    historial_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calificacion_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo_cambio = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    campo_modificado = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    valor_anterior = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valor_nuevo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    observacion = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    fecha_cambio = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCalificacion", x => x.historial_id);
                    table.CheckConstraint("CK_HistorialCalificacion_TipoCambio", "[tipo_cambio] IN ('CREACION','MODIFICACION','ANULACION','REEMPLAZO','OBSERVACION','APROBACION')");
                    table.ForeignKey(
                        name: "FK_HistorialCalificacion_CalificacionTributaria_calificacion_id",
                        column: x => x.calificacion_id,
                        principalTable: "CalificacionTributaria",
                        principalColumn: "calificacion_id");
                    table.ForeignKey(
                        name: "FK_HistorialCalificacion_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "ValidacionTributaria",
                columns: table => new
                {
                    validacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calificacion_id = table.Column<int>(type: "int", nullable: true),
                    archivo_carga_id = table.Column<int>(type: "int", nullable: true),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    resultado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    fecha_validacion = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidacionTributaria", x => x.validacion_id);
                    table.CheckConstraint("CK_ValidacionTributaria_Referencia", "[calificacion_id] IS NOT NULL OR [archivo_carga_id] IS NOT NULL");
                    table.CheckConstraint("CK_ValidacionTributaria_Resultado", "[resultado] IN ('VALIDADO','OBSERVADO','RECHAZADO','APROBADO')");
                    table.ForeignKey(
                        name: "FK_ValidacionTributaria_ArchivoCarga_archivo_carga_id",
                        column: x => x.archivo_carga_id,
                        principalTable: "ArchivoCarga",
                        principalColumn: "archivo_carga_id");
                    table.ForeignKey(
                        name: "FK_ValidacionTributaria_CalificacionTributaria_calificacion_id",
                        column: x => x.calificacion_id,
                        principalTable: "CalificacionTributaria",
                        principalColumn: "calificacion_id");
                    table.ForeignKey(
                        name: "FK_ValidacionTributaria_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivoCarga_plantilla_id",
                table: "ArchivoCarga",
                column: "plantilla_id");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivoCarga_tipo_carga_estado_carga",
                table: "ArchivoCarga",
                columns: new[] { "tipo_carga", "estado_carga" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivoCarga_usuario_id_fecha_carga",
                table: "ArchivoCarga",
                columns: new[] { "usuario_id", "fecha_carga" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_entidad_afectada_fecha_accion",
                table: "Auditoria",
                columns: new[] { "entidad_afectada", "fecha_accion" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_usuario_id",
                table: "Auditoria",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionTributaria_mercado_tipo_calificacion",
                table: "CalificacionTributaria",
                columns: new[] { "mercado", "tipo_calificacion" });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionTributaria_periodo_tributario_estado_calificacion",
                table: "CalificacionTributaria",
                columns: new[] { "periodo_tributario", "estado_calificacion" });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionTributaria_usuario_creador_id",
                table: "CalificacionTributaria",
                column: "usuario_creador_id");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionTributaria_vigente_desde_vigente_hasta",
                table: "CalificacionTributaria",
                columns: new[] { "vigente_desde", "vigente_hasta" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCargaMasiva_archivo_carga_id_numero_fila",
                table: "DetalleCargaMasiva",
                columns: new[] { "archivo_carga_id", "numero_fila" });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleCargaMasiva_calificacion_id",
                table: "DetalleCargaMasiva",
                column: "calificacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorCargaMasiva_archivo_carga_id_numero_fila",
                table: "ErrorCargaMasiva",
                columns: new[] { "archivo_carga_id", "numero_fila" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCalificacion_calificacion_id_fecha_cambio",
                table: "HistorialCalificacion",
                columns: new[] { "calificacion_id", "fecha_cambio" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCalificacion_usuario_id",
                table: "HistorialCalificacion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Permiso_codigo",
                table: "Permiso",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaCarga_tipo_carga_activa",
                table: "PlantillaCarga",
                columns: new[] { "tipo_carga", "activa" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaCarga_tipo_carga_version_plantilla",
                table: "PlantillaCarga",
                columns: new[] { "tipo_carga", "version_plantilla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReporteTributario_usuario_id_generado_en",
                table: "ReporteTributario",
                columns: new[] { "usuario_id", "generado_en" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Respaldo_usuario_id",
                table: "Respaldo",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_nombre",
                table: "Rol",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermiso_permiso_id",
                table: "RolPermiso",
                column: "permiso_id");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermiso_rol_id_permiso_id",
                table: "RolPermiso",
                columns: new[] { "rol_id", "permiso_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_email",
                table: "Usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_rol_id",
                table: "Usuario",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_ValidacionTributaria_archivo_carga_id_fecha_validacion",
                table: "ValidacionTributaria",
                columns: new[] { "archivo_carga_id", "fecha_validacion" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ValidacionTributaria_calificacion_id_fecha_validacion",
                table: "ValidacionTributaria",
                columns: new[] { "calificacion_id", "fecha_validacion" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ValidacionTributaria_usuario_id",
                table: "ValidacionTributaria",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "DetalleCargaMasiva");

            migrationBuilder.DropTable(
                name: "ErrorCargaMasiva");

            migrationBuilder.DropTable(
                name: "HistorialCalificacion");

            migrationBuilder.DropTable(
                name: "ReporteTributario");

            migrationBuilder.DropTable(
                name: "Respaldo");

            migrationBuilder.DropTable(
                name: "RolPermiso");

            migrationBuilder.DropTable(
                name: "ValidacionTributaria");

            migrationBuilder.DropTable(
                name: "Permiso");

            migrationBuilder.DropTable(
                name: "ArchivoCarga");

            migrationBuilder.DropTable(
                name: "CalificacionTributaria");

            migrationBuilder.DropTable(
                name: "PlantillaCarga");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
