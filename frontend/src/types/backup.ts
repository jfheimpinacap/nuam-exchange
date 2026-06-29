export type BackupType = 'Completo'|'Diferencial'|'Configuración';
export type BackupScope = 'Base de datos'|'Configuración del sistema'|'Archivos de carga'|'Todos los módulos';
export type BackupStatus = 'Completado'|'En proceso'|'Fallido'|'Programado'|'Cancelado';
export type BackupTrigger = 'Manual'|'Programado';
export interface BackupRecord { id:string; fechaInicio:string; fechaTermino:string; tipo:BackupType; alcance:BackupScope; tamañoBytes:number; estado:BackupStatus; ejecutadoPor:string; origen:BackupTrigger; descripcion:string; registrosIncluidos:number; observaciones:string; }
export interface BackupFilters { tipo:'Todos'|BackupType; alcance:'Todos'|BackupScope; estado:'Todos'|BackupStatus; origen:'Todos'|BackupTrigger; desde:string; hasta:string; texto:string; }
export interface BackupPolicy { frecuencia:'Diaria'|'Semanal'|'Mensual'; horaProgramada:string; retencionDias:number; tipoPredeterminado:BackupType; alcancePredeterminado:BackupScope; }
export interface BackupSimulationState { id:string; kind:'backup'|'restore'; progress:number; message:string; }
export type BackupSortKey = 'id'|'fechaInicio'|'tipo'|'alcance'|'tamañoBytes'|'estado'|'origen'|'ejecutadoPor';
export interface BackupSortState { key: BackupSortKey; direction:'asc'|'desc'; }
