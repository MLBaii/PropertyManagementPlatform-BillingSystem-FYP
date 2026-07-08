import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  name: string;
};

export function Avatar({ name }: Props) {
  const initial = name.trim().charAt(0).toUpperCase() || '?';

  return (
    <View style={styles.avatar}>
      <Text style={styles.initial}>{initial}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  avatar: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: colors.accentSoft,
    borderWidth: 1,
    borderColor: colors.accentLine,
    alignItems: 'center',
    justifyContent: 'center',
  },
  initial: {
    fontFamily: fonts.heading,
    fontSize: 24,
    color: colors.accent,
  },
});
