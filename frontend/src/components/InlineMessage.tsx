interface InlineMessageProps { tone?: 'info' | 'warning' | 'error' | 'success'; message: string; }
export function InlineMessage({ tone = 'info', message }: InlineMessageProps) {
  return <div className={`inline-message inline-${tone}`} role="status" aria-live="polite">{message}</div>;
}
