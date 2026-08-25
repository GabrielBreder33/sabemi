import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { DashboardPage } from './DashboardPage'

vi.mock('../hooks/usePayments', () => ({
  usePayments: () => ({
    data: { items: [], page: 1, pageSize: 20, totalItems: 0, totalPages: 0 },
    isLoading: false,
    error: null,
    refresh: vi.fn(),
  }),
}))

describe('DashboardPage', () => {
  it('shows empty state when no payments match filters', () => {
    render(<DashboardPage />)

    expect(screen.getByRole('heading', { name: /visão geral/i })).toBeInTheDocument()
    expect(screen.getByText('Nenhum pagamento encontrado')).toBeInTheDocument()
  })
})
