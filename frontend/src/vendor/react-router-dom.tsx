import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
type LocationState = unknown;
interface RouterContextValue { path: string; state?: LocationState; navigate: (to: string, options?: { replace?: boolean; state?: LocationState }) => void; }
const RouterContext = createContext<RouterContextValue | undefined>(undefined);
function normalize(path: string) { return path.startsWith('/') ? path : `/${path}`; }
export function BrowserRouter({ children }: { children: ReactNode }) { const [path,setPath]=useState(location.pathname === '/' ? '/' : location.pathname); const [state,setState]=useState<LocationState>(); const navigate=(to:string,options?:{replace?:boolean;state?:LocationState})=>{const next=normalize(to); window.history[options?.replace?'replaceState':'pushState'](options?.state ?? null,'',next); setPath(next); setState(options?.state);}; const value=useMemo(()=>({path,state,navigate}),[path,state]); return <RouterContext.Provider value={value}>{children}</RouterContext.Provider>; }
export function useNavigate() { const context=useContext(RouterContext); if(!context) throw new Error('useNavigate must be used inside BrowserRouter'); return context.navigate; }
export function useLocation() { const context=useContext(RouterContext); if(!context) throw new Error('useLocation must be used inside BrowserRouter'); return { pathname: context.path, state: context.state }; }
export function Link({ to, children, className }: { to:string; children:ReactNode; className?:string }) { const navigate=useNavigate(); return <a className={className} href={to} onClick={e=>{e.preventDefault(); navigate(to);}}>{children}</a>; }
export function NavLink({ to, children }: { to:string; children:ReactNode }) { const context=useContext(RouterContext); if(!context) throw new Error('NavLink must be used inside BrowserRouter'); return <a className={context.path.startsWith(to)?'active':''} href={to} onClick={e=>{e.preventDefault(); context.navigate(to);}}>{children}</a>; }
export function Navigate({ to, replace, state }: { to:string; replace?:boolean; state?:LocationState }) { const navigate=useNavigate(); navigate(to,{replace,state}); return null; }
