import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { ProofStatusBadge } from '@/components/pay/ProofStatusBadge';
import { Card } from '@/components/ui/Card';
import { PaymentProof } from '@/services/paymentProofs/paymentProofService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';
import { formatShortDate } from '@/utils/formatDate';

type Props = {
  proof: PaymentProof;
};

export function ProofHistoryCard({ proof }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const billRefs = proof.taggedBills.map((b) => b.referenceNumber).join(', ');
  const total = proof.taggedBills.reduce((sum, b) => sum + b.amount, 0);

  return (
    <Card style={styles.card}>
      <View style={styles.topRow}>
        <Text style={styles.date}>Submitted {formatShortDate(proof.submittedAt)}</Text>
        <ProofStatusBadge status={proof.status} />
      </View>
      <Text style={styles.billRefs} numberOfLines={1}>
        Tagged to: {billRefs || '—'}
      </Text>
      <View style={styles.bottomRow}>
        <Text style={styles.amount}>RM {total.toFixed(2)}</Text>
        {proof.adminRemarks && <Text style={styles.remarks}>{proof.adminRemarks}</Text>}
      </View>
    </Card>
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
    date: {
      fontFamily: fonts.body,
      fontSize: 12,
      color: colors.textSecondary,
    },
    billRefs: {
      fontFamily: fonts.mono,
      fontSize: 11,
      color: colors.textSecondary,
      marginBottom: 8,
    },
    bottomRow: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
    },
    amount: {
      fontFamily: fonts.heading,
      fontSize: 18,
      color: colors.text,
    },
    remarks: {
      fontFamily: fonts.body,
      fontSize: 11,
      color: colors.textSecondary,
      flexShrink: 1,
      textAlign: 'right',
    },
  });
