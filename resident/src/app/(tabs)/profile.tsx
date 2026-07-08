import React from 'react';
import { StyleSheet, Text } from 'react-native';

import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { useAuth } from '@/services/auth/AuthContext';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

export default function ProfileScreen() {
  const { resident, logout } = useAuth();

  return (
    <Screen style={styles.container}>
      <Text style={styles.name}>{resident?.fullName}</Text>
      <Text style={styles.email}>{resident?.email}</Text>
      <PrimaryButton label="Log Out" onPress={logout} />
    </Screen>
  );
}

const styles = StyleSheet.create({
  container: {
    justifyContent: 'center',
    gap: 16,
  },
  name: {
    fontFamily: fonts.heading,
    fontSize: 22,
    color: colors.text,
  },
  email: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    marginBottom: 20,
  },
});
