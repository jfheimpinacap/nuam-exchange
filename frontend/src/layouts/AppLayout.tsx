import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';
export function AppLayout({ children }: { children: React.ReactNode }) { return <div className="shell"><Sidebar /><main className="content"><Header />{children}</main></div>; }
