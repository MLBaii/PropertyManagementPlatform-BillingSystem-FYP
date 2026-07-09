import axios from 'axios';
import { router } from 'expo-router';
import * as SecureStore from 'expo-secure-store';

import { API_BASE_URL } from '@/config/env';
import { clearSession } from '@/services/auth/secureStorage';
import { notifySessionExpired } from '@/services/auth/sessionEvents';
import { AUTH_TOKEN_KEY } from '@/services/auth/storageKeys';

const LOGIN_ENDPOINT = '/auth/resident/login';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
});

apiClient.interceptors.request.use(async (config) => {
  const token = await SecureStore.getItemAsync(AUTH_TOKEN_KEY);
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  return config;
});

// UC-101 A6: a 401 on any authenticated call means the session is no longer
// valid (typically an expired token) — clear it and send the resident back to
// Login with an explanation, rather than leaving them stuck on a screen that
// will never load. Excludes the login endpoint itself, whose own 401s mean
// "wrong password" and are handled inline by the Login screen.
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const isUnauthorized = axios.isAxiosError(error) && error.response?.status === 401;
    const isLoginRequest = error.config?.url?.includes(LOGIN_ENDPOINT);

    if (isUnauthorized && !isLoginRequest) {
      await clearSession();
      notifySessionExpired();
      router.replace({ pathname: '/(auth)/login', params: { sessionExpired: '1' } });
    }

    return Promise.reject(error);
  }
);
