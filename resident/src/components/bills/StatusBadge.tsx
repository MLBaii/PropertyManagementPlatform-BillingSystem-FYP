import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { BillStatus } from '@/services/bills/billsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  status: BillStatus;
};

const STATUS_CONFIG: Record<BillStatus, { label: string; color: string; bg: string }> = {
  Paid: { label: 'Paid', color: colors.success, bg: colors.successBg },
  Unpaid: { label: 'Unpaid', color: colors.unpaid, bg: colors.unpaidBg },
  Overdue: { label: 'Overdue', color: colors.danger, bg: colors.dangerBg },
  ProofSubmitted: { label: 'Proof Submitted', color: colors.pending, bg: colors.pendingBg },
};

export function StatusBadge({ status }: Props) {
  const config = STATUS_CONFIG[status];

  return (
    <View style={[styles.badge, { backgroundColor: config.bg }]}>
      <View style={[styles.dot, { backgroundColor: config.color }]} />
      <Text style={[styles.label, { color: config.color }]}>{config.label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  badge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 5,
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 20,
  },
  dot: {
    width: 6,
    height: 6,
    borderRadius: 3,
  },
  label: {
    fontFamily: fonts.bodySemiBold,
    fontSize: 11,
  },
});
