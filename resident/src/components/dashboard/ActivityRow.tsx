import { Feather } from '@expo/vector-icons';
import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { ActivityItem } from '@/services/dashboard/dashboardService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  activity: ActivityItem;
  showBorder?: boolean;
};

export function ActivityRow({ activity, showBorder = true }: Props) {
  const isPayment = activity.type === 'PaymentConfirmed';
  const amountColor = isPayment ? colors.success : colors.textSecondary;
  const sign = isPayment ? '−' : '+';

  return (
    <View style={[styles.row, showBorder && styles.rowBorder]}>
      <View style={styles.iconWrap}>
        <Feather name={isPayment ? 'check-circle' : 'file-text'} size={15} color={colors.textSecondary} />
      </View>
      <View style={styles.textCol}>
        <Text style={styles.description}>{activity.description}</Text>
        {activity.reference && <Text style={styles.reference}>{activity.reference}</Text>}
      </View>
      <Text style={[styles.amount, { color: amountColor }]}>
        {sign}RM {activity.amount.toFixed(2)}
      </Text>
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 12,
  },
  rowBorder: {
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  iconWrap: {
    width: 30,
    height: 30,
    borderRadius: 15,
    backgroundColor: colors.surface2,
    alignItems: 'center',
    justifyContent: 'center',
  },
  textCol: {
    flex: 1,
  },
  description: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.text,
  },
  reference: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    marginTop: 2,
  },
  amount: {
    fontFamily: fonts.mono,
    fontSize: 12,
  },
});
