import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import { StatusBadge } from '@/components/bills/StatusBadge';
import { Card } from '@/components/ui/Card';
import { Bill } from '@/services/bills/billsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { getCountdownColor, getCountdownLabel, shouldShowDueDateLine } from '@/utils/billStatus';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';

type Props = {
  bill: Bill;
  onPress: () => void;
};

export function BillCard({ bill, onPress }: Props) {
  const countdownColor = getCountdownColor(bill.status);
  const countdownLabel = getCountdownLabel(bill);
  const showDueDate = shouldShowDueDateLine(bill.status);

  return (
    <Pressable onPress={onPress}>
      <Card style={styles.card}>
        <View style={styles.topRow}>
          <Text style={styles.reference}>{bill.referenceNumber}</Text>
          <View style={styles.badgeRow}>
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

const styles = StyleSheet.create({
  card: {
    marginBottom: 12,
  },
  topRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  badgeRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  reference: {
    fontFamily: fonts.mono,
    fontSize: 11,
    color: colors.textSecondary,
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
