import { EmptyState, ErrorState, LoadingState } from './ViewStates';
export function DataTableShell({ loading, error, empty, children }: { loading:boolean; error?:string; empty:boolean; children:React.ReactNode }) { if(loading) return <LoadingState />; if(error) return <ErrorState message={error} />; if(empty) return <EmptyState />; return <div className="table-wrap">{children}</div>; }
