import { Feather } from '@expo/vector-icons';
import React from 'react';
import { Pressable, StyleSheet, Text } from 'react-native';

import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  label: string;
  onPress: () => void;
  destructive?: boolean;
  showBorder?: boolean;
};

export function SettingsLinkRow({ label, onPress, destructive, showBorder = true }: Props) {
  return (
    <Pressable
      onPress={onPress}
      style={[styles.row, showBorder && styles.rowBorder]}
    >
      <Text style={[styles.label, destructive && styles.destructiveLabel]}>{label}</Text>
      <Feather name="chevron-right" size={16} color={destructive ? colors.danger : colors.textSecondary} />
    </Pressable>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 15,
  },
  rowBorder: {
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  label: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.text,
  },
  destructiveLabel: {
    color: colors.danger,
  },
});
