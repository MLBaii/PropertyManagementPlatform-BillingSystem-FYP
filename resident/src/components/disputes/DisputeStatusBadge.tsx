import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { DisputeStatus } from '@/services/disputes/disputesService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

type Props = {
  status: DisputeStatus;
};

const getStatusConfig = (colors: ThemeColors): Record<DisputeStatus, { label: string; color: string; bg: string }> => ({
  Open: { label: 'Open', color: colors.pending, bg: colors.pendingBg },
  UnderReview: { label: 'Under Review', color: colors.unpaid, bg: colors.unpaidBg },
  Resolved: { label: 'Resolved', color: colors.success, bg: colors.successBg },
  Rejected: { label: 'Rejected', color: colors.danger, bg: colors.dangerBg },
});

export function DisputeStatusBadge({ status }: Props) {
  const { colors } = useTheme();
  const config = getStatusConfig(colors)[status];

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
