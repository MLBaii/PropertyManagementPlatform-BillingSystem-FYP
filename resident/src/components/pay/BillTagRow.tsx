import { Feather } from '@expo/vector-icons';
import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { formatBillingPeriod } from '@/utils/formatDate';

type Props = {
  referenceNumber: string;
  billingPeriod: string;
  amount: number;
  checked: boolean;
  onToggle: () => void;
};

export function BillTagRow({ referenceNumber, billingPeriod, amount, checked, onToggle }: Props) {
  return (
    <Pressable onPress={onToggle} style={[styles.row, checked && styles.rowSelected]}>
      <View style={[styles.box, checked && styles.boxOn]}>
        {checked && <Feather name="check" size={12} color={colors.onAccent} />}
      </View>
      <View style={styles.textCol}>
        <Text style={styles.reference}>{referenceNumber}</Text>
        <Text style={styles.period}>
          {formatBillingPeriod(billingPeriod)} · RM {amount.toFixed(2)}
        </Text>
      </View>
      {checked && <Text style={styles.amount}>RM {amount.toFixed(2)}</Text>}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 12,
    paddingHorizontal: 14,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 14,
    backgroundColor: colors.surface,
    marginBottom: 8,
  },
  rowSelected: {
    borderColor: colors.accentLine,
    backgroundColor: colors.accentSoft,
  },
  box: {
    width: 20,
    height: 20,
    borderRadius: 6,
    borderWidth: 1.5,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  boxOn: {
    backgroundColor: colors.accent,
    borderColor: colors.accent,
  },
  textCol: {
    flex: 1,
  },
  reference: {
    fontFamily: fonts.mono,
    fontSize: 12,
    color: colors.text,
  },
  period: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    marginTop: 2,
  },
  amount: {
    fontFamily: fonts.mono,
    fontSize: 12,
    color: colors.accent,
  },
});
