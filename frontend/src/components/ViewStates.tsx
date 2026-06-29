export function LoadingState() { return <div className="state">Cargando información...</div>; }
export function EmptyState({ message='No hay resultados para mostrar.' }: { message?: string }) { return <div className="state empty">{message}</div>; }
export function ErrorState({ message }: { message: string }) { return <div className="state error">{message}</div>; }
