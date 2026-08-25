import { useState } from 'react'
import type { PaymentFilters as Filters } from '../types/payment'

interface Props {
  value: Filters
  onChange: (value: Filters) => void
}

export function PaymentFilters({ value, onChange }: Props) {
  const [draft, setDraft] = useState(value)

  return (
    <form className="filters" onSubmit={(event) => { event.preventDefault(); onChange(draft) }}>
      <p className="filter-heading">Filtrar movimentações</p>
      <label>
        Contrato
        <input aria-label="Contrato" value={draft.contractId} onChange={(event) => setDraft({ ...draft, contractId: event.target.value })} placeholder="CTR-987654" />
      </label>
      <label>
        Status do pagamento
        <select aria-label="Status do pagamento" value={draft.status} onChange={(event) => setDraft({ ...draft, status: event.target.value as Filters['status'] })}>
          <option value="">Todos</option>
          <option value="Sucesso">Sucesso</option>
          <option value="Erro">Erro</option>
        </select>
      </label>
      <label>
        Processamento
        <select aria-label="Status do processamento" value={draft.processingStatus} onChange={(event) => setDraft({ ...draft, processingStatus: event.target.value as Filters['processingStatus'] })}>
          <option value="">Todos</option>
          <option value="Pending">Pendente</option>
          <option value="Processing">Processando</option>
          <option value="Processed">Processado</option>
          <option value="Failed">Erro</option>
        </select>
      </label>
      <button type="submit">Filtrar</button>
    </form>
  )
}
