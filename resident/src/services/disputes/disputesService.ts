import axios from 'axios';

import { apiClient } from '@/services/api/client';

export type DisputeStatus = 'Open' | 'UnderReview' | 'Resolved' | 'Rejected';

export type Dispute = {
  disputeId: number;
  billId: number;
  billReferenceNumber: string;
  billingPeriod: string;
  reason: string;
  status: DisputeStatus;
  submittedAt: string;
  resolvedAt: string | null;
  adminResponse: string | null;
};

export const MIN_DISPUTE_REASON_LENGTH = 20;
export const DISPUTE_REASON_ERROR_MESSAGE = 'Please provide a dispute reason of at least 20 characters.';

// Thrown by submitDispute on a 409 — carries the bill's existing active dispute so the
// caller can show it directly instead of a generic error.
export class ActiveDisputeExistsError extends Error {
  dispute: Dispute;

  constructor(dispute: Dispute) {
    super('This bill already has an active dispute. Please wait for it to be resolved.');
    this.dispute = dispute;
  }
}

export async function getDisputes(status?: DisputeStatus): Promise<Dispute[]> {
  const { data } = await apiClient.get<Dispute[]>('/residents/disputes', {
    params: status ? { status } : undefined,
  });
  return data;
}

export async function getDisputeById(disputeId: number): Promise<Dispute> {
  const { data } = await apiClient.get<Dispute>(`/residents/disputes/${disputeId}`);
  return data;
}

export async function submitDispute(billId: number, reason: string): Promise<Dispute> {
  try {
    const { data } = await apiClient.post<Dispute>('/residents/disputes', { billId, reason });
    return data;
  } catch (err) {
    if (axios.isAxiosError(err) && err.response?.status === 409 && err.response.data?.dispute) {
      throw new ActiveDisputeExistsError(err.response.data.dispute as Dispute);
    }
    throw err;
  }
}
