import { Feather } from '@expo/vector-icons';
import React from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';

import { Card } from '@/components/ui/Card';
import { Receipt } from '@/services/receipts/receiptsService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';

type Props = {
  receipt: Receipt;
  onDownload: () => void;
  isGenerating: boolean;
  error?: string;
};

export function ReceiptCard({ receipt, onDownload, isGenerating, error }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  return (
    <Card style={styles.card}>
      <View style={styles.topRow}>
        <Text style={styles.reference}>{receipt.receiptNumber}</Text>
        <Text style={styles.amount}>RM {receipt.amount.toFixed(2)}</Text>
      </View>
      <Text style={styles.billInfo}>
        {receipt.billReferenceNumber} · {formatBillingPeriod(receipt.billingPeriod)}
      </Text>
      <Text style={styles.date}>
        {receipt.channel} · {formatShortDate(receipt.paymentDate)}
      </Text>

      <Pressable onPress={onDownload} disabled={isGenerating} style={styles.downloadRow} hitSlop={6}>
        {isGenerating ? (
          <ActivityIndicator size="small" color={colors.accent} />
        ) : (
          <Feather name="download" size={14} color={colors.accent} />
        )}
        <Text style={styles.downloadLabel}>{isGenerating ? 'Generating…' : 'Download Receipt'}</Text>
      </Pressable>
      {error && <Text style={styles.errorText}>{error}</Text>}
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
    reference: {
      fontFamily: fonts.mono,
      fontSize: 12,
      color: colors.text,
    },
    amount: {
      fontFamily: fonts.heading,
      fontSize: 16,
      color: colors.success,
    },
    billInfo: {
      fontFamily: fonts.body,
      fontSize: 12,
      color: colors.text,
      marginBottom: 3,
    },
    date: {
      fontFamily: fonts.mono,
      fontSize: 10,
      color: colors.textSecondary,
      marginBottom: 12,
    },
    downloadRow: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 7,
      alignSelf: 'flex-start',
    },
    downloadLabel: {
      fontFamily: fonts.bodyMedium,
      fontSize: 12,
      color: colors.accent,
    },
    errorText: {
      fontFamily: fonts.body,
      fontSize: 11,
      color: colors.danger,
      marginTop: 6,
    },
  });
