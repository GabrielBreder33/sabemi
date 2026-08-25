import { StatusBadge } from './StatusBadge'
import type { PaymentItem } from '../types/payment'

const currency = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
const date = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

export function PaymentTable({ items, isLoading }: { items: PaymentItem[]; isLoading: boolean }) {
  if (isLoading) return <p className="state-message">Carregando pagamentos...</p>
  if (items.length === 0) return <p className="state-message">Nenhum pagamento encontrado</p>

  return (
    <div className="table-wrap">
      <table>
        <thead><tr><th>Transação</th><th>Contrato</th><th>Valor</th><th>Data</th><th>Status</th><th>Processamento</th><th>Detalhe</th></tr></thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td><code>{item.transactionId}</code></td>
              <td>{item.contractId}</td>
              <td>{currency.format(item.amount)}</td>
              <td>{date.format(new Date(item.paymentDate))}</td>
              <td><StatusBadge value={item.paymentStatus} /></td>
              <td><StatusBadge value={item.processingStatus} /></td>
              <td>{item.errorMessage ? <span className="error-detail" title={item.errorMessage}>{item.errorMessage}</span> : '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
