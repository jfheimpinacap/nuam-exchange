export function StatusBadge({ value }: { value: string }) { return <span className={`status status-${value.toLowerCase().split(' ').join('-')}`}>{value}</span>; }
