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
      <aside className="app-sidebar">
        <div className="brand-lockup">
          <span className="brand-mark" aria-hidden="true">S</span>
          <div>
            <span className="brand-name">SABEMI</span>
            <span className="brand-caption">Central de pagamentos</span>
          </div>
        </div>
        <p className="sidebar-label">Navegação</p>
        <nav className="sidebar-nav" aria-label="Navegação principal">
          <span className="sidebar-link sidebar-link-active" aria-current="page">Visão geral</span>
        </nav>
        <div className="sidebar-status">
          <span className="sidebar-status-label">Status do sistema</span>
          <span className={`sidebar-status-value${error ? ' sidebar-status-value-offline' : ''}`}>{error ? 'API indisponível' : 'API operacional'}</span>
          <span className="sidebar-environment">Ambiente local</span>
        </div>
      </aside>
      <section className="app-main">
        <header className="page-header">
          <div>
            <p className="eyebrow">SABEMI PRIME · OPERAÇÕES</p>
            <h1>Visão geral</h1>
            <p className="subtitle">Acompanhe os pagamentos recebidos e o processamento dos contratos em um só lugar.</p>
            <p className="header-meta">Atualização automática · 5s</p>
          </div>
          <button className="refresh-button" type="button" onClick={() => void refresh()}>Atualizar dados</button>
        </header>
        <section className="summary-grid" aria-label="Resumo dos pagamentos">
          <article className="summary-card summary-card-accent">
            <span className="summary-card-label">Total recebido</span>
            <strong className="summary-card-value">{data.totalItems}</strong>
            <span className="summary-card-detail">eventos registrados</span>
          </article>
          <article className="summary-card">
            <span className="summary-card-label">Processados</span>
            <strong className="summary-card-value">{processed}</strong>
            <span className="summary-card-detail">nesta página</span>
          </article>
          <article className="summary-card">
            <span className="summary-card-label">Em análise</span>
            <strong className="summary-card-value">{pending}</strong>
            <span className="summary-card-detail">pendentes ou em curso</span>
          </article>
          <article className="summary-card">
            <span className="summary-card-label">Falhas</span>
            <strong className="summary-card-value">{failed}</strong>
            <span className="summary-card-detail">exigem atenção</span>
          </article>
        </section>
        <section className="panel" aria-labelledby="payments-title">
          <div className="panel-heading">
            <div>
              <h2 className="panel-title" id="payments-title">Movimentações recentes</h2>
              <p className="panel-description">Consulte e filtre os eventos recebidos pela operação.</p>
            </div>
          </div>
          <PaymentFilters value={filters} onChange={handleFilters} />
          {error && <p className="error-banner" role="alert">{error}</p>}
          <PaymentTable items={data.items} isLoading={isLoading} />
          {data.totalPages > 0 && <nav className="pagination" aria-label="Paginação"><button disabled={page <= 1} onClick={() => setPage((current) => current - 1)}>Anterior</button><span>Página {page} de {data.totalPages}</span><button disabled={page >= data.totalPages} onClick={() => setPage((current) => current + 1)}>Próxima</button></nav>}
        </section>
      </section>
    </main>
  )
}
