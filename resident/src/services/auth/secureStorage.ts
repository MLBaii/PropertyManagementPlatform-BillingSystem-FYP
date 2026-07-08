import * as SecureStore from 'expo-secure-store';

import { AUTH_TOKEN_KEY } from '@/services/api/client';

const RESIDENT_INFO_KEY = 'propertybill_resident_info';

export type StoredResident = {
  residentId: number;
  fullName: string;
  email: string;
  unitId: number;
};

export async function saveSession(token: string, resident: StoredResident): Promise<void> {
  await SecureStore.setItemAsync(AUTH_TOKEN_KEY, token);
  await SecureStore.setItemAsync(RESIDENT_INFO_KEY, JSON.stringify(resident));
}

export async function loadSession(): Promise<{ token: string; resident: StoredResident } | null> {
  const [token, residentJson] = await Promise.all([
    SecureStore.getItemAsync(AUTH_TOKEN_KEY),
    SecureStore.getItemAsync(RESIDENT_INFO_KEY),
  ]);

  if (!token || !residentJson) {
    return null;
  }

  return { token, resident: JSON.parse(residentJson) as StoredResident };
}

export async function clearSession(): Promise<void> {
  await Promise.all([
    SecureStore.deleteItemAsync(AUTH_TOKEN_KEY),
    SecureStore.deleteItemAsync(RESIDENT_INFO_KEY),
  ]);
}
