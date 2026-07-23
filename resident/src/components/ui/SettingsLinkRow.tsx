import { Feather } from '@expo/vector-icons';
import React from 'react';
import { Pressable, StyleSheet, Text } from 'react-native';

import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

type Props = {
  label: string;
  onPress: () => void;
  destructive?: boolean;
  showBorder?: boolean;
};

export function SettingsLinkRow({ label, onPress, destructive, showBorder = true }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);

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

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
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
