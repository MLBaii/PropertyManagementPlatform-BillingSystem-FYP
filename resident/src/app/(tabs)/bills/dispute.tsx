import { router, useLocalSearchParams } from 'expo-router';
import React, { useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';

import { StatusBadge } from '@/components/bills/StatusBadge';
import { DisputeStatusBadge } from '@/components/disputes/DisputeStatusBadge';
import { Card } from '@/components/ui/Card';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { TextField } from '@/components/ui/TextField';
import { BillDetail, getBillById } from '@/services/bills/billsService';
import {
  ActiveDisputeExistsError,
  Dispute,
  DISPUTE_REASON_ERROR_MESSAGE,
  getDisputes,
  MIN_DISPUTE_REASON_LENGTH,
  submitDispute,
} from '@/services/disputes/disputesService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { formatBillingPeriod } from '@/utils/formatDate';

export default function DisputeBillScreen() {
  const { billId } = useLocalSearchParams<{ billId: string }>();
  const numericBillId = Number(billId);

  const [bill, setBill] = useState<BillDetail | null>(null);
  const [existingDispute, setExistingDispute] = useState<Dispute | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();

  const [reason, setReason] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | undefined>();
  const [submittedDispute, setSubmittedDispute] = useState<Dispute | null>(null);

  useEffect(() => {
    if (!Number.isFinite(numericBillId)) {
      setLoadError('Invalid bill.');
      setIsLoading(false);
      return;
    }

    Promise.all([getBillById(numericBillId), getDisputes()])
      .then(([billDetail, disputes]) => {
        setBill(billDetail);
        const active = disputes.find(
          (d) => d.billId === numericBillId && (d.status === 'Open' || d.status === 'UnderReview')
        );
        setExistingDispute(active ?? null);
      })
      .catch(() => setLoadError('Could not load this bill. Please try again.'))
      .finally(() => setIsLoading(false));
  }, [numericBillId]);

  const trimmedLength = reason.trim().length;
  const meetsMinLength = trimmedLength >= MIN_DISPUTE_REASON_LENGTH;
  const canSubmit = meetsMinLength && !isSubmitting;

  const countColor = useMemo(
    () => (meetsMinLength ? colors.success : colors.textSecondary),
    [meetsMinLength]
  );

  const handleSubmit = async () => {
    if (!canSubmit) {
      return;
    }
    setSubmitError(undefined);
    setIsSubmitting(true);
    try {
      const dispute = await submitDispute(numericBillId, reason.trim());
      setSubmittedDispute(dispute);
    } catch (err) {
      if (err instanceof ActiveDisputeExistsError) {
        setExistingDispute(err.dispute);
      } else {
        setSubmitError('Could not submit your dispute. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <Screen style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </Screen>
    );
  }

  if (!bill || loadError) {
    return (
      <Screen style={styles.centered}>
        <Text style={styles.loadError}>{loadError ?? 'Could not load this bill.'}</Text>
      </Screen>
    );
  }

  const activeOrSubmitted = submittedDispute ?? existingDispute;

  if (activeOrSubmitted) {
    return (
      <Screen>
        <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
          <View style={styles.header}>
            <Text
              style={styles.backLink}
              onPress={() => router.back()}
            >
              ‹ Bill
            </Text>
            <Text style={styles.title}>Dispute Bill</Text>
          </View>

          <Card style={styles.confirmationCard}>
            <Text style={styles.eyebrow}>
              {submittedDispute ? 'Dispute Submitted' : 'Active Dispute'}
            </Text>
            <View style={styles.confirmationTopRow}>
              <Text style={styles.confirmationBillRef}>{activeOrSubmitted.billReferenceNumber}</Text>
              <DisputeStatusBadge status={activeOrSubmitted.status} />
            </View>
            <Text style={styles.confirmationReasonLabel}>Reason</Text>
            <Text style={styles.confirmationReason}>{activeOrSubmitted.reason}</Text>
            <Text style={styles.confirmationReasonLabel}>Admin Response</Text>
            <Text style={styles.confirmationAdminResponse}>
              {activeOrSubmitted.adminResponse ?? 'The admin will review and respond through the system.'}
            </Text>
          </Card>

          <View style={styles.doneWrap}>
            <PrimaryButton label="Done" onPress={() => router.replace('/(tabs)/bills')} />
          </View>
        </ScrollView>
      </Screen>
    );
  }

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.header}>
          <Text style={styles.backLink} onPress={() => router.back()}>
            ‹ Bill
          </Text>
          <Text style={styles.title}>Dispute Bill</Text>
        </View>

        <Card style={styles.summaryCard}>
          <Text style={styles.eyebrow}>Disputing</Text>
          <View style={styles.summaryRow}>
            <View>
              <Text style={styles.summaryReference}>{bill.referenceNumber}</Text>
              <Text style={styles.summaryPeriod}>
                {formatBillingPeriod(bill.billingPeriod)} · RM {bill.outstandingBalance.toFixed(2)}
              </Text>
            </View>
            <StatusBadge status={bill.status} />
          </View>
        </Card>

        <TextField
          label="Reason for dispute"
          placeholder="Explain what looks wrong on this bill…"
          value={reason}
          onChangeText={setReason}
          multiline
          numberOfLines={5}
          style={styles.textarea}
        />
        <View style={styles.countRow}>
          <Text style={styles.minLengthNote}>Minimum {MIN_DISPUTE_REASON_LENGTH} characters</Text>
          <Text style={[styles.countText, { color: countColor }]}>
            {trimmedLength} / {MIN_DISPUTE_REASON_LENGTH}
            {meetsMinLength ? ' ✓' : ''}
          </Text>
        </View>

        {submitError && <Text style={styles.errorText}>{submitError}</Text>}

        <View style={styles.submitWrap}>
          <PrimaryButton label="Submit Dispute" onPress={handleSubmit} loading={isSubmitting} disabled={!canSubmit} />
          <Text style={styles.footerNote}>The admin will review and respond through the system</Text>
        </View>
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
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
  header: {
    marginBottom: 16,
  },
  backLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.accent,
    marginBottom: 12,
  },
  title: {
    fontFamily: fonts.heading,
    fontSize: 24,
    letterSpacing: -0.48,
    color: colors.text,
  },
  eyebrow: {
    fontFamily: fonts.body,
    fontSize: 10,
    letterSpacing: 1.5,
    textTransform: 'uppercase',
    color: colors.textSecondary,
    marginBottom: 8,
  },
  summaryCard: {
    marginBottom: 18,
  },
  summaryRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  summaryReference: {
    fontFamily: fonts.mono,
    fontSize: 11,
    color: colors.textSecondary,
  },
  summaryPeriod: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.text,
    marginTop: 2,
  },
  textarea: {
    height: 110,
    paddingTop: 12,
    textAlignVertical: 'top',
  },
  countRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginTop: -8,
    marginBottom: 18,
  },
  minLengthNote: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
  },
  countText: {
    fontFamily: fonts.mono,
    fontSize: 11,
  },
  errorText: {
    fontFamily: fonts.body,
    fontSize: 12,
    color: colors.danger,
    marginBottom: 12,
  },
  submitWrap: {
    gap: 4,
  },
  footerNote: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    textAlign: 'center',
    marginTop: 8,
  },
  confirmationCard: {
    marginBottom: 18,
  },
  confirmationTopRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 14,
  },
  confirmationBillRef: {
    fontFamily: fonts.mono,
    fontSize: 13,
    color: colors.text,
  },
  confirmationReasonLabel: {
    fontFamily: fonts.body,
    fontSize: 10,
    letterSpacing: 1,
    textTransform: 'uppercase',
    color: colors.textSecondary,
    marginBottom: 5,
    marginTop: 10,
  },
  confirmationReason: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.text,
    lineHeight: 19,
  },
  confirmationAdminResponse: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    lineHeight: 19,
  },
  doneWrap: {
    marginTop: 4,
  },
});
