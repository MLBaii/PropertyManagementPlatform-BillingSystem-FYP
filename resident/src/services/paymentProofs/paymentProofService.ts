import { apiClient } from '@/services/api/client';
import { PickedFile } from '@/utils/proofFilePicker';

export type ProofStatus = 'Pending' | 'Approved' | 'Rejected';

export type TaggedBill = {
  billId: number;
  referenceNumber: string;
  billingPeriod: string;
  amount: number;
};

export type PaymentProof = {
  proofId: number;
  fileUrl: string;
  fileType: string;
  fileSize: number;
  status: ProofStatus;
  adminRemarks: string | null;
  submittedAt: string;
  reviewedAt: string | null;
  taggedBills: TaggedBill[];
};

export async function getPaymentProofs(): Promise<PaymentProof[]> {
  const { data } = await apiClient.get<PaymentProof[]>('/residents/payment-proofs');
  return data;
}

export async function submitPaymentProof(file: PickedFile, billIds: number[]): Promise<PaymentProof> {
  const formData = new FormData();
  // React Native's FormData accepts this {uri, name, type} shape for file parts — not the
  // spec-compliant web File API, but it's what RN's networking layer expects.
  formData.append('File', {
    uri: file.uri,
    name: file.name,
    type: file.mimeType,
  } as unknown as Blob);
  billIds.forEach((billId) => formData.append('BillIds', String(billId)));

  // Deliberately no explicit Content-Type header — axios/RN needs to generate the multipart
  // boundary itself from the FormData body, and setting Content-Type manually here would
  // strip that boundary and break parsing server-side.
  const { data } = await apiClient.post<PaymentProof>('/residents/payment-proofs', formData);
  return data;
}
