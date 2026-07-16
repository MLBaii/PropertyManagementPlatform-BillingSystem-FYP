import React, { createContext, useCallback, useContext, useEffect, useState } from 'react';

import { registerForPushNotificationsAsync } from '@/utils/pushNotifications';

import { login as loginRequest } from './authService';
import { clearSession, loadSession, StoredResident } from './secureStorage';
import { setSessionExpiredHandler } from './sessionEvents';

type AuthContextValue = {
  resident: StoredResident | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [resident, setResident] = useState<StoredResident | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadSession()
      .then((session) => setResident(session?.resident ?? null))
      .finally(() => setIsLoading(false));
  }, []);

  useEffect(() => {
    // Bridges the axios response interceptor's 401 handling (client.ts, outside
    // the React tree) into this context's in-memory state.
    setSessionExpiredHandler(() => setResident(null));
    return () => setSessionExpiredHandler(null);
  }, []);

  useEffect(() => {
    // Covers both "on login" (login() below sets resident) and "on app start" (the
    // loadSession effect above sets resident from a restored session) with one effect,
    // since both paths converge on `resident` becoming non-null. Registration is entirely
    // non-blocking (UC-101 A5) — see registerForPushNotificationsAsync's own error handling.
    if (resident) {
      void registerForPushNotificationsAsync();
    }
  }, [resident?.residentId]);

  const login = useCallback(async (email: string, password: string) => {
    const loggedInResident = await loginRequest(email, password);
    setResident(loggedInResident);
  }, []);

  const logout = useCallback(async () => {
    await clearSession();
    setResident(null);
  }, []);

  return (
    <AuthContext.Provider value={{ resident, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return ctx;
}
