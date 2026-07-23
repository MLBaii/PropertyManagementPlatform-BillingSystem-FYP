import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import { StatusBadge } from '@/components/bills/StatusBadge';
import { Card } from '@/components/ui/Card';
import { Bill } from '@/services/bills/billsService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';
import { getCountdownColor, getCountdownLabel, shouldShowDueDateLine } from '@/utils/billStatus';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';

type Props = {
  bill: Bill;
  onPress: () => void;
};

export function BillCard({ bill, onPress }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const countdownColor = getCountdownColor(bill.status, colors);
  const countdownLabel = getCountdownLabel(bill);
  const showDueDate = shouldShowDueDateLine(bill.status);

  return (
    <Pressable onPress={onPress}>
      <Card style={styles.card}>
        <View style={styles.topRow}>
          <Text style={styles.reference}>{bill.referenceNumber}</Text>
          <View style={styles.badgeCol}>
            <StatusBadge status={bill.status} />
            {bill.activeDisputeStatus && <StatusBadge status={bill.activeDisputeStatus} />}
          </View>
        </View>
        <View style={styles.bottomRow}>
          <View>
            <Text style={styles.period}>{formatBillingPeriod(bill.billingPeriod)}</Text>
            <Text style={styles.amount}>RM {bill.totalAmount.toFixed(2)}</Text>
          </View>
          <View style={styles.rightCol}>
            <Text style={[styles.countdown, { color: countdownColor }]}>{countdownLabel}</Text>
            {showDueDate && (
              <Text style={styles.dueDate}>Due {formatShortDate(bill.dueDate)}</Text>
            )}
          </View>
        </View>
      </Card>
    </Pressable>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    card: {
      marginBottom: 14,
      padding: 18,
    },
    topRow: {
      flexDirection: 'row',
      alignItems: 'flex-start',
      justifyContent: 'space-between',
      gap: 12,
      marginBottom: 12,
    },
    // Payment badge (always) stacked above the dispute badge (when present), right-aligned —
    // two badges side by side on one row got cramped next to a long reference number, so the
    // second badge now drops to a line of its own instead of fighting for horizontal space.
    badgeCol: {
      flexDirection: 'column',
      alignItems: 'flex-end',
      gap: 6,
    },
    reference: {
      fontFamily: fonts.mono,
      fontSize: 11,
      color: colors.textSecondary,
      flexShrink: 1,
    },
    bottomRow: {
      flexDirection: 'row',
      alignItems: 'flex-end',
      justifyContent: 'space-between',
    },
    period: {
      fontFamily: fonts.body,
      fontSize: 12,
      color: colors.textSecondary,
    },
    amount: {
      fontFamily: fonts.heading,
      fontSize: 22,
      color: colors.text,
      marginTop: 2,
    },
    rightCol: {
      alignItems: 'flex-end',
    },
    countdown: {
      fontFamily: fonts.mono,
      fontSize: 11,
    },
    dueDate: {
      fontFamily: fonts.body,
      fontSize: 10,
      color: colors.textSecondary,
      marginTop: 2,
    },
  });
