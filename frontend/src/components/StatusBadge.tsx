const labels: Record<string, { label: string; title: string }> = {
  Pending: { label: 'Pendente', title: 'Aguardando processamento' },
  Processing: { label: 'Processando', title: 'Processamento em andamento' },
  Processed: { label: 'Processado', title: 'Processamento concluído' },
  Failed: { label: 'Erro', title: 'Falha no processamento' },
  Sucesso: { label: 'Sucesso', title: 'Pagamento com sucesso' },
  Erro: { label: 'Erro', title: 'Pagamento com erro' },
}

export function StatusBadge({ value }: { value: string }) {
  const status = labels[value] ?? { label: value, title: value }
  return <span className={`status-badge status-${value.toLowerCase()}`} role="status" title={status.title}>{status.label}</span>
}
