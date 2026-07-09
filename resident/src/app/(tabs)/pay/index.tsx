import { Feather } from '@expo/vector-icons';
import { router } from 'expo-router';
import React, { useCallback, useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { ProofHistoryCard } from '@/components/pay/ProofHistoryCard';
import { Screen } from '@/components/ui/Screen';
import { getPaymentProofs, PaymentProof } from '@/services/paymentProofs/paymentProofService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

export default function PayScreen() {
  const [proofs, setProofs] = useState<PaymentProof[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();

  const load = useCallback(() => {
    setLoadError(undefined);
    return getPaymentProofs()
      .then(setProofs)
      .catch(() => setLoadError('Could not load your submission history. Please try again.'));
  }, []);

  useEffect(() => {
    load().finally(() => setIsLoading(false));
  }, [load]);

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.header}>
          <Text style={styles.eyebrow}>Payments</Text>
          <Text style={styles.title}>Pay</Text>
        </View>

        <Pressable
          style={({ pressed }) => [styles.uploadCta, pressed && styles.uploadCtaPressed]}
          onPress={() => router.push('/(tabs)/pay/upload')}
        >
          <View style={styles.uploadIcon}>
            <Feather name="upload" size={18} color={colors.onAccent} />
          </View>
          <View style={styles.uploadTextCol}>
            <Text style={styles.uploadTitle}>Upload Payment Proof</Text>
            <Text style={styles.uploadSubtitle}>Attach a receipt and tag it to your bill(s)</Text>
          </View>
          <Feather name="chevron-right" size={18} color={colors.onAccent} />
        </Pressable>

        <Text style={styles.sectionEyebrow}>Submission History</Text>

        {isLoading ? (
          <View style={styles.centered}>
            <ActivityIndicator color={colors.accent} />
          </View>
        ) : loadError ? (
          <View style={styles.centered}>
            <Text style={styles.errorText}>{loadError}</Text>
            <Text style={styles.retryLink} onPress={() => load()}>
              Try again
            </Text>
          </View>
        ) : proofs.length === 0 ? (
          <View style={styles.centered}>
            <Text style={styles.emptyText}>You haven't submitted any payment proofs yet.</Text>
          </View>
        ) : (
          proofs.map((proof) => <ProofHistoryCard key={proof.proofId} proof={proof} />)
        )}
      </ScrollView>
    </Screen>
  );
}

const styles = StyleSheet.create({
  scrollContent: {
    paddingTop: 10,
    paddingBottom: 32,
  },
  header: {
    marginVertical: 4,
    marginBottom: 18,
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
  },
  uploadCta: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    backgroundColor: colors.accent,
    borderRadius: 16,
    padding: 16,
    marginBottom: 26,
  },
  uploadCtaPressed: {
    opacity: 0.85,
  },
  uploadIcon: {
    width: 36,
    height: 36,
    borderRadius: 10,
    backgroundColor: 'rgba(26, 20, 16, 0.18)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  uploadTextCol: {
    flex: 1,
  },
  uploadTitle: {
    fontFamily: fonts.bodySemiBold,
    fontSize: 14,
    color: colors.onAccent,
  },
  uploadSubtitle: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.onAccent,
    opacity: 0.8,
    marginTop: 2,
  },
  sectionEyebrow: {
    fontFamily: fonts.body,
    fontSize: 10,
    letterSpacing: 1.5,
    textTransform: 'uppercase',
    color: colors.textSecondary,
    marginBottom: 10,
  },
  centered: {
    alignItems: 'center',
    paddingTop: 24,
    gap: 10,
  },
  errorText: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: 'center',
  },
  retryLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.accent,
  },
  emptyText: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: 'center',
  },
});
