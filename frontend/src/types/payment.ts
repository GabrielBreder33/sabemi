export type PaymentStatus = '' | 'Sucesso' | 'Erro'
export type ProcessingStatus = '' | 'Pending' | 'Processing' | 'Processed' | 'Failed' | 'ValidationFailed'

export interface PaymentFilters {
  contractId: string
  status: PaymentStatus
  processingStatus: ProcessingStatus
}

export interface PaymentItem {
  id: string
  transactionId: string | null
  contractId: string | null
  amount: number | null
  paymentDate: string | null
  paymentStatus: Exclude<PaymentStatus, ''> | null
  processingStatus: Exclude<ProcessingStatus, ''>
  receivedAt: string
  processedAt: string | null
  errorMessage: string | null
  rawPayload?: string | null
}

export interface PagedPayments {
  items: PaymentItem[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}
