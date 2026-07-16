import { apiClient } from '@/services/api/client';

export type Receipt = {
  paymentId: number;
  receiptNumber: string;
  amount: number;
  paymentDate: string;
  channel: string;
  billReferenceNumber: string;
  billingPeriod: string;
};

export type ReceiptDetail = Receipt & {
  unitNumber: string;
  propertyName: string;
};

export async function getReceipts(): Promise<Receipt[]> {
  const { data } = await apiClient.get<Receipt[]>('/residents/receipts');
  return data;
}

export async function getReceiptById(paymentId: number): Promise<ReceiptDetail> {
  const { data } = await apiClient.get<ReceiptDetail>(`/residents/receipts/${paymentId}`);
  return data;
}
