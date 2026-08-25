import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { StatusBadge } from './StatusBadge'

describe('StatusBadge', () => {
  it('shows failed processing with an accessible error label', () => {
    render(<StatusBadge value="Failed" />)

    expect(screen.getByText('Erro')).toBeInTheDocument()
    expect(screen.getByRole('status')).toHaveAttribute('title', 'Falha no processamento')
  })

  it('shows processed payment status', () => {
    render(<StatusBadge value="Processed" />)

    expect(screen.getByText('Processado')).toBeInTheDocument()
  })
})
