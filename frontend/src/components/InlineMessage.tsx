export function InlineMessage({ type='info', children }: { type?: 'info' | 'error' | 'success'; children: React.ReactNode }) { return <div className={`inline-message ${type}`}>{children}</div>; }
