export const moneyFormatter = new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 });
export const numberFormatter = new Intl.NumberFormat('es-CL');
export function toCsvCell(value: unknown) { const text = String(value ?? '').replace(/"/g, '""'); return /^[=+\-@]/.test(text) ? `"'${text}"` : `"${text}"`; }
export function downloadCsv(name: string, rows: unknown[][]) { const content = `\uFEFF${rows.map(row=>row.map(toCsvCell).join(';')).join('\n')}`; const blob = new Blob([content], { type: 'text/csv;charset=utf-8' }); const url = URL.createObjectURL(blob); const a = document.createElement('a'); a.href=url; a.download=name; a.click(); URL.revokeObjectURL(url); }
