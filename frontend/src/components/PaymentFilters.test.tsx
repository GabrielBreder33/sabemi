import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { PaymentFilters } from './PaymentFilters'

describe('PaymentFilters', () => {
  it('submits the selected contract and status filters', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<PaymentFilters value={{ contractId: '', status: '', processingStatus: '' }} onChange={onChange} />)

    await user.type(screen.getByLabelText('Contrato'), 'CTR-1')
    await user.selectOptions(screen.getByLabelText('Status do pagamento'), 'Sucesso')
    await user.click(screen.getByRole('button', { name: 'Filtrar' }))

    expect(onChange).toHaveBeenCalledWith({ contractId: 'CTR-1', status: 'Sucesso', processingStatus: '' })
  })
})
