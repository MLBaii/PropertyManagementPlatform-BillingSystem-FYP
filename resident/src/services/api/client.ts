import axios from 'axios';
import * as SecureStore from 'expo-secure-store';

import { API_BASE_URL } from '@/config/env';

export const AUTH_TOKEN_KEY = 'propertybill_auth_token';

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
