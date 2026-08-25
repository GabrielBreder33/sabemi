export type PaymentStatus = '' | 'Sucesso' | 'Erro'
export type ProcessingStatus = '' | 'Pending' | 'Processing' | 'Processed' | 'Failed'

export interface PaymentFilters {
  contractId: string
  status: PaymentStatus
  processingStatus: ProcessingStatus
}

export interface PaymentItem {
  id: string
  transactionId: string
  contractId: string
  amount: number
  paymentDate: string
  paymentStatus: Exclude<PaymentStatus, ''>
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
