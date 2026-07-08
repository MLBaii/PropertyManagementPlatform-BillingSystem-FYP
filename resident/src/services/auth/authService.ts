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
