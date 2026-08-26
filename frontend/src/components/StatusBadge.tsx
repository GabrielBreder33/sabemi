const labels: Record<string, { label: string; title: string }> = {
  Pending: { label: 'Pendente', title: 'Aguardando processamento' },
  Processing: { label: 'Processando', title: 'Processamento em andamento' },
  Processed: { label: 'Processado', title: 'Processamento concluído' },
  Failed: { label: 'Erro', title: 'Falha no processamento' },
  ValidationFailed: { label: 'Falha de validação', title: 'Falha de validação do webhook' },
  Sucesso: { label: 'Sucesso', title: 'Pagamento com sucesso' },
  Erro: { label: 'Erro', title: 'Pagamento com erro' },
}

export function StatusBadge({ value }: { value: string | null }) {
  const statusKey = value ?? 'unknown'
  const status = labels[statusKey] ?? { label: value ?? 'Não informado', title: value ?? 'Não informado' }
  return <span className={`status-badge status-${statusKey.toLowerCase()}`} role="status" title={status.title}><span aria-hidden="true" />{status.label}</span>
}
