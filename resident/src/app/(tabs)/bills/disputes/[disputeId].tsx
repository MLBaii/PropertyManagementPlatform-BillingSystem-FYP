import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import React, { useCallback, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';

import { DisputeStatusBadge } from '@/components/disputes/DisputeStatusBadge';
import { Card } from '@/components/ui/Card';
import { Screen } from '@/components/ui/Screen';
import { Dispute, getDisputeById } from '@/services/disputes/disputesService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';

export default function DisputeDetailScreen() {
  const { disputeId } = useLocalSearchParams<{ disputeId: string }>();
  const numericDisputeId = Number(disputeId);

  const [dispute, setDispute] = useState<Dispute | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();

  useFocusEffect(
    useCallback(() => {
      if (!Number.isFinite(numericDisputeId)) {
        setLoadError('Invalid dispute.');
        setIsLoading(false);
        return;
      }

      getDisputeById(numericDisputeId)
        .then((fresh) => {
          setDispute(fresh);
          setLoadError(undefined);
        })
        .catch(() => setLoadError('Could not load this dispute.'))
        .finally(() => setIsLoading(false));
    }, [numericDisputeId])
  );

  if (isLoading) {
    return (
      <Screen style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </Screen>
    );
  }

  if (!dispute) {
    return (
      <Screen style={styles.centered}>
        <Text style={styles.loadError}>{loadError}</Text>
      </Screen>
    );
  }

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.topRow}>
          <Text style={styles.backLink} onPress={() => router.back()}>
            ‹ Disputes
          </Text>
          <DisputeStatusBadge status={dispute.status} />
        </View>

        <Text style={styles.eyebrow}>{dispute.billReferenceNumber}</Text>
        <Text style={styles.title}>{formatBillingPeriod(dispute.billingPeriod)}</Text>
        <Text style={styles.dates}>
          Submitted {formatShortDate(dispute.submittedAt)}
          {dispute.resolvedAt ? ` · Resolved ${formatShortDate(dispute.resolvedAt)}` : ''}
        </Text>

        <Card style={styles.card}>
          <Text style={styles.sectionLabel}>Reason</Text>
          <Text style={styles.reason}>{dispute.reason}</Text>
        </Card>

        <Card style={styles.card}>
          <Text style={styles.sectionLabel}>Admin Response</Text>
          <Text style={dispute.adminResponse ? styles.adminResponse : styles.adminResponsePlaceholder}>
            {dispute.adminResponse ?? 'The admin will review and respond through the system.'}
          </Text>
        </Card>
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
  topRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 14,
  },
  backLink: {
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
  dates: {
    fontFamily: fonts.mono,
    fontSize: 11,
    color: colors.textSecondary,
    marginBottom: 18,
  },
  card: {
    marginBottom: 16,
  },
  sectionLabel: {
    fontFamily: fonts.body,
    fontSize: 10,
    letterSpacing: 1,
    textTransform: 'uppercase',
    color: colors.textSecondary,
    marginBottom: 8,
  },
  reason: {
    fontFamily: fonts.body,
    fontSize: 14,
    color: colors.text,
    lineHeight: 20,
  },
  adminResponse: {
    fontFamily: fonts.body,
    fontSize: 14,
    color: colors.text,
    lineHeight: 20,
  },
  adminResponsePlaceholder: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    fontStyle: 'italic',
    lineHeight: 19,
  },
});
