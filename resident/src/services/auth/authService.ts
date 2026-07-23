import { apiClient } from '@/services/api/client';

import { saveSession, StoredResident } from './secureStorage';

type LoginApiResponse = {
  token: string;
  residentId: number;
  fullName: string;
  email: string;
  unitId: number;
};

export async function login(email: string, password: string): Promise<StoredResident> {
  const { data } = await apiClient.post<LoginApiResponse>('/auth/resident/login', {
    email,
    password,
  });

  const resident: StoredResident = {
    residentId: data.residentId,
    fullName: data.fullName,
    email: data.email,
    unitId: data.unitId,
  };

  await saveSession(data.token, resident);
  return resident;
}

// UC-101 A1. Always resolves with the same generic message regardless of whether the email
// matched a resident — the backend deliberately doesn't distinguish, to prevent account
// enumeration — so there's nothing for this function to branch on beyond a network failure.
export async function forgotPassword(email: string): Promise<string> {
  const { data } = await apiClient.post<{ message: string }>('/auth/resident/forgot-password', {
    email,
  });
  return data.message;
}

export async function resetPassword(token: string, newPassword: string): Promise<string> {
  const { data } = await apiClient.post<{ message: string }>('/auth/resident/reset-password', {
    token,
    newPassword,
  });
  return data.message;
}
