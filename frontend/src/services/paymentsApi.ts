import type { PaymentFilters, PagedPayments } from '../types/payment'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api'

export async function getPayments(filters: PaymentFilters, page = 1, pageSize = 20): Promise<PagedPayments> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  if (filters.contractId) params.set('contratoId', filters.contractId)
  if (filters.status) params.set('status', filters.status)
  if (filters.processingStatus) params.set('processingStatus', filters.processingStatus)

  const response = await fetch(`${apiBaseUrl}/pagamentos?${params.toString()}`)
  if (!response.ok) throw new Error('Não foi possível carregar os pagamentos.')
  return response.json() as Promise<PagedPayments>
}
