import React, { createContext, useCallback, useContext, useEffect, useState } from 'react';

import { login as loginRequest } from './authService';
import { clearSession, loadSession, StoredResident } from './secureStorage';

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
