import React from 'react';
import { StyleSheet, Text } from 'react-native';

import { Screen } from '@/components/ui/Screen';
import { useAuth } from '@/services/auth/AuthContext';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

// Proves login worked end-to-end. Replaced by the real dashboard
// (balances, recent activity) once UC-102 lands.
export default function HomeScreen() {
  const { resident } = useAuth();

  return (
    <Screen style={styles.container}>
      <Text style={styles.greeting}>Welcome, {resident?.fullName ?? 'Resident'}</Text>
      <Text style={styles.subtitle}>{resident?.email}</Text>
    </Screen>
  );
}

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
  },
  greeting: {
    fontFamily: fonts.heading,
    fontSize: 24,
    color: colors.text,
    textAlign: 'center',
  },
  subtitle: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
  },
});
