import { apiClient } from '@/services/api/client';

// Purely payment-side — never folds in dispute state. See DisputeStatus for that.
export type PaymentStatus = 'Unpaid' | 'Overdue' | 'Paid' | 'ProofSubmitted';

// Only set while a dispute on the bill is still active (Open/UnderReview) — a second,
// independent badge alongside PaymentStatus, not a replacement for it.
export type DisputeStatus = 'Disputed' | 'PendingDispute';

// Combined type for anywhere a single badge value is rendered (StatusBadge accepts either).
export type BillStatus = PaymentStatus | DisputeStatus;

export type Bill = {
  billId: number;
  referenceNumber: string;
  billingPeriod: string;
  issueDate: string;
  dueDate: string;
  totalAmount: number;
  outstandingBalance: number;
  status: PaymentStatus;
  activeDisputeStatus: DisputeStatus | null;
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
  unitNumber: string;
  propertyName: string;
};

// status here only ever matches PaymentStatus — the backend's ?status= filter compares
// against the payment-only Status column, which never holds a DisputeStatus value.
export async function getBills(status?: PaymentStatus): Promise<Bill[]> {
  const { data } = await apiClient.get<Bill[]>('/residents/bills', {
    params: status ? { status } : undefined,
  });
  return data;
}

export async function getBillById(billId: number): Promise<BillDetail> {
  const { data } = await apiClient.get<BillDetail>(`/residents/bills/${billId}`);
  return data;
}
