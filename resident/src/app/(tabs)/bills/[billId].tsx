import { Feather } from '@expo/vector-icons';
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import React, { useCallback, useRef, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { StatusBadge } from '@/components/bills/StatusBadge';
import { Card } from '@/components/ui/Card';
import { GhostButton } from '@/components/ui/GhostButton';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { useAuth } from '@/services/auth/AuthContext';
import { BillDetail, getBillById } from '@/services/bills/billsService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';
import { getCountdownColor, getCountdownLabel, shouldShowDueDateLine } from '@/utils/billStatus';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';
import { generateAndShareBillPdf } from '@/utils/generateBillPdf';

export default function BillDetailScreen() {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const { billId } = useLocalSearchParams<{ billId: string }>();
  const { resident } = useAuth();
  const [bill, setBill] = useState<BillDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();
  const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);
  const [pdfError, setPdfError] = useState<string | undefined>();
  const lastLoadedBillId = useRef<string | undefined>(undefined);

  // Refetches on every focus, not just first mount — the status shown here (e.g. after
  // tagging this bill to a payment proof on the Pay tab) would otherwise stay stale until
  // the app reloads, since Expo Router keeps this screen mounted in the background stack.
  useFocusEffect(
    useCallback(() => {
      const id = Number(billId);
      if (!Number.isFinite(id)) {
        setLoadError('Invalid bill.');
        setIsLoading(false);
        return;
      }

      if (lastLoadedBillId.current !== billId) {
        lastLoadedBillId.current = billId;
        setIsLoading(true);
        getBillById(id)
          .then(setBill)
          .catch(() => setLoadError('Could not load this bill.'))
          .finally(() => setIsLoading(false));
        return;
      }

      getBillById(id)
        .then((fresh) => {
          setBill(fresh);
          setLoadError(undefined);
        })
        .catch(() => setLoadError('Could not load this bill.'));
    }, [billId])
  );

  if (isLoading) {
    return (
      <Screen style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </Screen>
    );
  }

  if (!bill) {
    return (
      <Screen style={styles.centered}>
        <Text style={styles.loadError}>{loadError}</Text>
      </Screen>
    );
  }

  const countdownColor = getCountdownColor(bill.status, colors);
  const countdownLabel = getCountdownLabel(bill);
  const showDueDate = shouldShowDueDateLine(bill.status);

  const handleDownloadPdf = async () => {
    setPdfError(undefined);
    setIsGeneratingPdf(true);
    try {
      await generateAndShareBillPdf({ bill, residentName: resident?.fullName ?? 'Resident' });
    } catch {
      setPdfError('Unable to generate PDF. Please try again.');
    } finally {
      setIsGeneratingPdf(false);
    }
  };

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.topRow}>
          <Pressable style={styles.backRow} onPress={() => router.back()} hitSlop={10}>
            <Feather name="chevron-left" size={16} color={colors.accent} />
            <Text style={styles.backText}>Bills</Text>
          </Pressable>
          <View style={styles.badgeRow}>
            <StatusBadge status={bill.status} />
            {bill.activeDisputeStatus && <StatusBadge status={bill.activeDisputeStatus} />}
          </View>
        </View>

        <Text style={styles.eyebrow}>{bill.referenceNumber}</Text>
        <Text style={styles.title}>{formatBillingPeriod(bill.billingPeriod)}</Text>
        <Text style={[styles.countdown, { color: countdownColor }]}>
          {countdownLabel}
          {showDueDate ? ` · Due ${formatShortDate(bill.dueDate)}` : ''}
        </Text>

        <Card style={styles.lineItemsCard}>
          {bill.lineItems.map((item) => {
            const isPenalty = item.lineItemType === 'Penalty';
            return (
              <View key={item.lineItemId} style={styles.lineItem}>
                <Text style={[styles.lineLabel, isPenalty && styles.penaltyText]}>
                  {item.description}
                </Text>
                <Text style={[styles.lineAmount, isPenalty && styles.penaltyText]}>
                  RM {item.amount.toFixed(2)}
                </Text>
              </View>
            );
          })}
          <View style={[styles.lineItem, styles.totalRow]}>
            <Text style={styles.totalLabel}>Total Due</Text>
            <Text style={styles.totalAmount}>RM {bill.outstandingBalance.toFixed(2)}</Text>
          </View>
        </Card>

        <View style={styles.actions}>
          <PrimaryButton
            label="Upload Payment Proof"
            onPress={() =>
              router.push({ pathname: '/(tabs)/pay/upload', params: { billId: String(bill.billId) } })
            }
          />
          <GhostButton label="Download PDF" onPress={handleDownloadPdf} loading={isGeneratingPdf} />
          {pdfError && <Text style={styles.pdfError}>{pdfError}</Text>}
          <Text
            style={styles.disputeLink}
            onPress={() =>
              router.push({ pathname: '/(tabs)/bills/dispute', params: { billId: String(bill.billId) } })
            }
          >
            Dispute This Bill
          </Text>
        </View>
      </ScrollView>
    </Screen>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    centered: {
      alignItems: 'center',
      justifyContent: 'center',
    },
    loadError: {
      fontFamily: fonts.body,
      fontSize: 13,
      color: colors.textSecondary,
      textAlign: 'center',
    },
    scrollContent: {
      paddingTop: 10,
      paddingBottom: 32,
    },
    topRow: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      marginBottom: 14,
    },
    backRow: {
      flexDirection: 'row',
      alignItems: 'center',
    },
    badgeRow: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 6,
    },
    backText: {
      fontFamily: fonts.bodyMedium,
      fontSize: 13,
      color: colors.accent,
    },
    eyebrow: {
      fontFamily: fonts.body,
      fontSize: 10,
      letterSpacing: 1.5,
      textTransform: 'uppercase',
      color: colors.textSecondary,
    },
    title: {
      fontFamily: fonts.heading,
      fontSize: 26,
      letterSpacing: -0.52,
      color: colors.text,
      marginTop: 2,
      marginBottom: 4,
    },
    countdown: {
      fontFamily: fonts.mono,
      fontSize: 11,
      marginBottom: 18,
    },
    lineItemsCard: {
      marginBottom: 16,
    },
    lineItem: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      paddingVertical: 11,
      borderBottomWidth: 1,
      borderBottomColor: colors.border,
    },
    lineLabel: {
      fontFamily: fonts.body,
      fontSize: 13.5,
      color: colors.textSecondary,
    },
    lineAmount: {
      fontFamily: fonts.mono,
      fontSize: 13.5,
      color: colors.text,
    },
    penaltyText: {
      color: colors.danger,
    },
    totalRow: {
      paddingTop: 13,
      borderBottomWidth: 0,
    },
    totalLabel: {
      fontFamily: fonts.bodySemiBold,
      fontSize: 14,
      color: colors.text,
    },
    totalAmount: {
      fontFamily: fonts.monoMedium,
      fontSize: 16,
      color: colors.accent,
    },
    actions: {
      gap: 14,
    },
    pdfError: {
      fontFamily: fonts.body,
      fontSize: 12,
      color: colors.danger,
      textAlign: 'center',
      marginTop: -6,
    },
    disputeLink: {
      fontFamily: fonts.bodyMedium,
      fontSize: 13,
      color: colors.textSecondary,
      textAlign: 'center',
      paddingTop: 6,
    },
  });
