import { Button } from './Button';

export function LoadingState({ message = 'Cargando registros...' }: { message?: string }) {
  return <div className="view-state" role="status" aria-live="polite"><span className="spinner" />{message}</div>;
}
export function ErrorState({ title, description }: { title: string; description: string }) {
  return <div className="view-state view-state-error" role="alert"><strong>{title}</strong><span>{description}</span></div>;
}
export function EmptyState({ title, description, actionLabel, onAction }: { title: string; description: string; actionLabel: string; onAction: () => void }) {
  return <div className="view-state"><strong>{title}</strong><span>{description}</span><Button onClick={onAction}>{actionLabel}</Button></div>;
}
