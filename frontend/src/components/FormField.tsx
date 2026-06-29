import type { ReactNode } from 'react';
interface FormFieldProps { id: string; label: string; children: ReactNode; }
export function FormField({ id, label, children }: FormFieldProps) {
  return <div className="form-field"><label htmlFor={id}>{label}</label>{children}</div>;
}
