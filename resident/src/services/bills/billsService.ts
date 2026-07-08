import { apiClient } from '@/services/api/client';

export type BillStatus = 'Unpaid' | 'Overdue' | 'Paid' | 'ProofSubmitted';

export type Bill = {
  billId: number;
  referenceNumber: string;
  billingPeriod: string;
  issueDate: string;
  dueDate: string;
  totalAmount: number;
  outstandingBalance: number;
  status: BillStatus;
  daysUntilDue: number;
};

export type BillLineItem = {
  lineItemId: number;
  description: string;
  amount: number;
  lineItemType: string;
};

export type BillDetail = Bill & {
  lineItems: BillLineItem[];
};

export async function getBills(status?: BillStatus): Promise<Bill[]> {
  const { data } = await apiClient.get<Bill[]>('/residents/bills', {
    params: status ? { status } : undefined,
  });
  return data;
}

export async function getBillById(billId: number): Promise<BillDetail> {
  const { data } = await apiClient.get<BillDetail>(`/residents/bills/${billId}`);
  return data;
}
