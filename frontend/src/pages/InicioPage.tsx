import { Link } from 'react-router-dom';
import { PageHeader } from '../components/PageHeader';
import { DemoNotice } from '../components/DemoNotice';
import { mockClassifications, uploadReviews } from '../mocks/classifications';
import { moneyFormatter } from '../utils/formatters';
export function InicioPage() { const total=mockClassifications.reduce((s,i)=>s+i.amount,0); return <section className="page"><PageHeader title="Inicio" description="Dashboard con indicadores simulados del sistema tributario." /><DemoNotice /><div className="stats"><article><span>Calificaciones</span><strong>{mockClassifications.length}</strong></article><article><span>Monto total</span><strong>{moneyFormatter.format(total)}</strong></article><article><span>Cargas registradas</span><strong>{uploadReviews.length}</strong></article></div><div className="cards"><Link to="/calificaciones/nueva">Nueva calificación</Link><Link to="/cargas/x-factor">Cargar X Factor</Link><Link to="/reportes">Ver reportes</Link></div></section>; }
