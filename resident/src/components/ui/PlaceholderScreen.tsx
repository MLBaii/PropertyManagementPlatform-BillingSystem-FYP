import React from 'react';
import { StyleSheet, Text } from 'react-native';

import { Screen } from '@/components/ui/Screen';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  title: string;
  subtitle?: string;
};

export function PlaceholderScreen({ title, subtitle }: Props) {
  return (
    <Screen style={styles.container}>
      <Text style={styles.title}>{title}</Text>
      {subtitle ? <Text style={styles.subtitle}>{subtitle}</Text> : null}
    </Screen>
  );
}

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
  },
  title: {
    fontFamily: fonts.heading,
    fontSize: 22,
    color: colors.text,
  },
  subtitle: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
  },
});
