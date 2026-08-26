import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { PaymentTable } from './PaymentTable'

describe('PaymentTable', () => {
  it('shows validation failures as visible alerts', () => {
    render(<PaymentTable
      isLoading={false}
      items={[{
        id: 'event-1',
        transactionId: null,
        contractId: null,
        amount: null,
        paymentDate: null,
        paymentStatus: null,
        processingStatus: 'ValidationFailed',
        receivedAt: '2026-08-26T10:00:00Z',
        processedAt: null,
        errorMessage: 'JSON inválido.',
        rawPayload: '{invalid-json',
      }]}
    />)

    expect(screen.getByRole('alert')).toHaveTextContent('JSON inválido.')
    expect(screen.getByText('Falha de validação')).toBeInTheDocument()
  })
})
