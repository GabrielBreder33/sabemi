import { useCallback, useEffect, useState } from 'react'
import { getPayments } from '../services/paymentsApi'
import type { PaymentFilters, PagedPayments } from '../types/payment'

const emptyPage: PagedPayments = { items: [], page: 1, pageSize: 20, totalItems: 0, totalPages: 0 }

export function usePayments(filters: PaymentFilters, page = 1) {
  const [data, setData] = useState<PagedPayments>(emptyPage)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const refresh = useCallback(async () => {
    setIsLoading(true)
    try {
      setData(await getPayments(filters, page))
      setError(null)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Erro ao carregar pagamentos.')
    } finally {
      setIsLoading(false)
    }
  }, [filters, page])

  useEffect(() => {
    void refresh()
    const timer = window.setInterval(() => void refresh(), 5000)
    return () => window.clearInterval(timer)
  }, [refresh])

  return { data, isLoading, error, refresh }
}
