import { StatusBadge } from './StatusBadge'
import type { PaymentItem } from '../types/payment'

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const date = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

export function PaymentTable({ items, isLoading }: { items: PaymentItem[]; isLoading: boolean }) {
  if (isLoading) return <div className="loading-table" aria-busy="true" aria-label="Carregando pagamentos">{[1, 2, 3].map((row) => <span className="loading-row" key={row} />)}</div>
  if (items.length === 0) return <p className="state-message"><strong>Nenhum pagamento encontrado</strong><span>Ajuste os filtros ou aguarde novos eventos.</span></p>

  return (
    <div className="table-wrap">
      <table>
        <caption>Eventos de pagamento recebidos</caption>
        <thead><tr><th>Transação</th><th>Contrato</th><th>Valor</th><th>Data</th><th>Status</th><th>Processamento</th><th>Detalhe</th></tr></thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td className="transaction-cell"><code>{item.transactionId ?? '—'}</code></td>
              <td className="contract-cell">{item.contractId ?? '—'}</td>
              <td className="money-cell">{item.amount === null ? '—' : currency.format(item.amount)}</td>
              <td className="date-cell">{item.paymentDate === null ? '—' : date.format(new Date(item.paymentDate))}</td>
              <td><StatusBadge value={item.paymentStatus} /></td>
              <td><StatusBadge value={item.processingStatus} /></td>
              <td>{item.errorMessage ? <span className={`error-detail${item.processingStatus === 'ValidationFailed' ? ' validation-alert' : ''}`} role={item.processingStatus === 'ValidationFailed' ? 'alert' : undefined} title={item.errorMessage}>{item.errorMessage}</span> : '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
