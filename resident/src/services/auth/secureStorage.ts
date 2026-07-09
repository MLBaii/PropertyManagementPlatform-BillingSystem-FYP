import * as SecureStore from 'expo-secure-store';

import { AUTH_TOKEN_KEY, RESIDENT_INFO_KEY } from './storageKeys';
import { isTokenExpired } from '@/utils/jwt';

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

  // UC-101 A6: don't silently resume an expired session — clear it and let the
  // caller fall through to the Login screen instead.
  if (isTokenExpired(token)) {
    await clearSession();
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
