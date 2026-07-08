import { Redirect, Stack } from 'expo-router';

import { useAuth } from '@/services/auth/AuthContext';

export default function AuthLayout() {
  const { resident, isLoading } = useAuth();

  if (isLoading) {
    return null;
  }

  if (resident) {
    return <Redirect href="/(tabs)/home" />;
  }

  return <Stack screenOptions={{ headerShown: false }} />;
}
