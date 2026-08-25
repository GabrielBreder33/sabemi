import { useState } from 'react'
import { PaymentFilters } from '../components/PaymentFilters'
import { PaymentTable } from '../components/PaymentTable'
import { usePayments } from '../hooks/usePayments'
import type { PaymentFilters as Filters } from '../types/payment'

const initialFilters: Filters = { contractId: '', status: '', processingStatus: '' }

export function DashboardPage() {
  const [filters, setFilters] = useState(initialFilters)
  const [page, setPage] = useState(1)
  const { data, isLoading, error, refresh } = usePayments(filters, page)
  const processed = data.items.filter((item) => item.processingStatus === 'Processed').length
  const failed = data.items.filter((item) => item.processingStatus === 'Failed').length
  const pending = data.items.filter((item) => item.processingStatus === 'Pending' || item.processingStatus === 'Processing').length

  const handleFilters = (nextFilters: Filters) => {
    setFilters(nextFilters)
    setPage(1)
  }

  return (
    <main className="app-shell">
      <header className="page-header">
        <div><p className="eyebrow">SABEMI · OPERAÇÕES</p><h1>Pagamentos</h1><p className="subtitle">Acompanhe os webhooks recebidos e o processamento dos contratos.</p></div>
        <button className="refresh-button" type="button" onClick={() => void refresh()}>Atualizar</button>
      </header>
      <section className="summary-grid" aria-label="Resumo dos pagamentos">
        <article><span>Total na página</span><strong>{data.items.length}</strong></article>
        <article><span>Processados</span><strong>{processed}</strong></article>
        <article><span>Em andamento</span><strong>{pending}</strong></article>
        <article><span>Com erro</span><strong>{failed}</strong></article>
      </section>
      <section className="panel">
        <PaymentFilters value={filters} onChange={handleFilters} />
        {error && <p className="error-banner" role="alert">{error}</p>}
        <PaymentTable items={data.items} isLoading={isLoading} />
        {data.totalPages > 0 && <nav className="pagination" aria-label="Paginação"><button disabled={page <= 1} onClick={() => setPage((current) => current - 1)}>Anterior</button><span>Página {page} de {data.totalPages}</span><button disabled={page >= data.totalPages} onClick={() => setPage((current) => current + 1)}>Próxima</button></nav>}
      </section>
    </main>
  )
}
