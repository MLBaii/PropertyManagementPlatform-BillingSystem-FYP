import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { ProofStatus } from '@/services/paymentProofs/paymentProofService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  status: ProofStatus;
};

const STATUS_CONFIG: Record<ProofStatus, { label: string; color: string; bg: string }> = {
  Pending: { label: 'Pending', color: colors.pending, bg: colors.pendingBg },
  Approved: { label: 'Approved', color: colors.success, bg: colors.successBg },
  Rejected: { label: 'Rejected', color: colors.danger, bg: colors.dangerBg },
};

export function ProofStatusBadge({ status }: Props) {
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
