export function DemoNotice({ compact = false }: { compact?: boolean }) {
  return <aside className={compact ? 'demo-notice demo-notice-compact' : 'demo-notice'} aria-label="Modo demostración"><strong>Modo demostración</strong><span>Datos ficticios · Sin persistencia · Sin conexión al backend</span></aside>;
}
