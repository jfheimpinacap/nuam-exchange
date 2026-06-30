const dateOnlyPattern = /^\d{4}-\d{2}-\d{2}$/;

export function formatIsoDateForDisplay(value: string, options?: { includeTime?: boolean }) {
  if (dateOnlyPattern.test(value)) {
    const [year, month, day] = value.split('-').map(Number);
    return new Intl.DateTimeFormat('es-CL', { dateStyle: 'medium' }).format(new Date(year, month - 1, day));
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat('es-CL', {
    dateStyle: 'medium',
    timeStyle: options?.includeTime ? 'short' : undefined,
  }).format(date);
}
