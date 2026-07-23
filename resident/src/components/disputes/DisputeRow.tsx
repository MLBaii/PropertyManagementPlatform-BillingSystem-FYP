import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import { Card } from '@/components/ui/Card';
import { Dispute } from '@/services/disputes/disputesService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';

import { DisputeStatusBadge } from './DisputeStatusBadge';

type Props = {
  dispute: Dispute;
  onPress: () => void;
};

export function DisputeRow({ dispute, onPress }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  return (
    <Pressable onPress={onPress}>
      <Card style={styles.card}>
        <View style={styles.topRow}>
          <Text style={styles.reference}>{dispute.billReferenceNumber}</Text>
          <DisputeStatusBadge status={dispute.status} />
        </View>
        <Text style={styles.period}>{formatBillingPeriod(dispute.billingPeriod)}</Text>
        <Text style={styles.reason} numberOfLines={2}>
          {dispute.reason}
        </Text>
        <Text style={styles.date}>Submitted {formatShortDate(dispute.submittedAt)}</Text>
      </Card>
    </Pressable>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    card: {
      marginBottom: 12,
    },
    topRow: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      marginBottom: 6,
    },
    reference: {
      fontFamily: fonts.mono,
      fontSize: 12,
      color: colors.text,
    },
    period: {
      fontFamily: fonts.body,
      fontSize: 12,
      color: colors.textSecondary,
      marginBottom: 8,
    },
    reason: {
      fontFamily: fonts.body,
      fontSize: 13,
      color: colors.text,
      lineHeight: 18,
      marginBottom: 8,
    },
    date: {
      fontFamily: fonts.mono,
      fontSize: 10,
      color: colors.textSecondary,
    },
  });
