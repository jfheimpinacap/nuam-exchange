import type { UploadConfig } from './uploadConfigs';
import type { UploadValidationError } from '../../types/upload';
import { downloadCsv } from '../../utils/csvExport';
export function downloadTemplate(config:UploadConfig){ downloadCsv(`plantilla-${config.type}.csv`, [config.examples[0]], config.columns.map((column, index)=>({ header: column.key, value: (row:string[])=>row[index] ?? '' }))); }
export function downloadErrors(type:string, errors:UploadValidationError[]){ downloadCsv(`errores-${type}.csv`, errors, [{header:'fila',value:e=>e.rowNumber},{header:'columna',value:e=>e.column},{header:'codigo',value:e=>e.code},{header:'mensaje',value:e=>e.message}]); }
export const formatBytes=(bytes:number)=> bytes < 1024 ? `${bytes} B` : bytes < 1024*1024 ? `${(bytes/1024).toFixed(1)} KB` : `${(bytes/1024/1024).toFixed(2)} MB`;
