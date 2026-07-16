import { Feather } from '@expo/vector-icons';
import { router, useFocusEffect } from 'expo-router';
import React, { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { ProofHistoryCard } from '@/components/pay/ProofHistoryCard';
import { ReceiptCard } from '@/components/receipts/ReceiptCard';
import { Screen } from '@/components/ui/Screen';
import { useAuth } from '@/services/auth/AuthContext';
import { getPaymentProofs, PaymentProof } from '@/services/paymentProofs/paymentProofService';
import { getReceiptById, getReceipts, Receipt } from '@/services/receipts/receiptsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { generateAndShareReceiptPdf } from '@/utils/generateReceiptPdf';

export default function PayScreen() {
  const { resident } = useAuth();

  const [proofs, setProofs] = useState<PaymentProof[]>([]);
  const [isLoadingProofs, setIsLoadingProofs] = useState(true);
  const [proofsError, setProofsError] = useState<string | undefined>();

  const [receipts, setReceipts] = useState<Receipt[]>([]);
  const [isLoadingReceipts, setIsLoadingReceipts] = useState(true);
  const [receiptsError, setReceiptsError] = useState<string | undefined>();

  const [generatingReceiptId, setGeneratingReceiptId] = useState<number | null>(null);
  const [receiptGenError, setReceiptGenError] = useState<{ id: number; message: string } | null>(null);

  // Tab screens stay mounted when you switch away — refetch on every focus (not just once on
  // mount) so a proof submitted or a payment confirmed elsewhere never shows stale here, same
  // fix pattern as Bills/Alerts/Disputes.
  const loadProofs = useCallback(() => {
    return getPaymentProofs()
      .then((fresh) => {
        setProofs(fresh);
        setProofsError(undefined);
      })
      .catch(() => setProofsError('Could not load your submission history. Please try again.'))
      .finally(() => setIsLoadingProofs(false));
  }, []);

  const loadReceipts = useCallback(() => {
    return getReceipts()
      .then((fresh) => {
        setReceipts(fresh);
        setReceiptsError(undefined);
      })
      .catch(() => setReceiptsError('Could not load your receipts. Please try again.'))
      .finally(() => setIsLoadingReceipts(false));
  }, []);

  useFocusEffect(
    useCallback(() => {
      void loadProofs();
      void loadReceipts();
    }, [loadProofs, loadReceipts])
  );

  const handleDownloadReceipt = async (receipt: Receipt) => {
    setReceiptGenError(null);
    setGeneratingReceiptId(receipt.paymentId);
    try {
      const detail = await getReceiptById(receipt.paymentId);
      await generateAndShareReceiptPdf({ receipt: detail, residentName: resident?.fullName ?? 'Resident' });
    } catch {
      setReceiptGenError({ id: receipt.paymentId, message: 'Unable to generate receipt. Please try again.' });
    } finally {
      setGeneratingReceiptId(null);
    }
  };

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

        {isLoadingProofs ? (
          <View style={styles.centered}>
            <ActivityIndicator color={colors.accent} />
          </View>
        ) : proofsError ? (
          <View style={styles.centered}>
            <Text style={styles.errorText}>{proofsError}</Text>
            <Text style={styles.retryLink} onPress={() => loadProofs()}>
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

        <Text style={[styles.sectionEyebrow, styles.receiptsEyebrow]}>Receipts</Text>

        {isLoadingReceipts ? (
          <View style={styles.centered}>
            <ActivityIndicator color={colors.accent} />
          </View>
        ) : receiptsError ? (
          <View style={styles.centered}>
            <Text style={styles.errorText}>{receiptsError}</Text>
            <Text style={styles.retryLink} onPress={() => loadReceipts()}>
              Try again
            </Text>
          </View>
        ) : receipts.length === 0 ? (
          <View style={styles.centered}>
            <Text style={styles.emptyText}>
              No receipts available yet. Receipts appear here once your payments are confirmed.
            </Text>
          </View>
        ) : (
          receipts.map((receipt) => (
            <ReceiptCard
              key={receipt.paymentId}
              receipt={receipt}
              onDownload={() => handleDownloadReceipt(receipt)}
              isGenerating={generatingReceiptId === receipt.paymentId}
              error={receiptGenError?.id === receipt.paymentId ? receiptGenError.message : undefined}
            />
          ))
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
  receiptsEyebrow: {
    marginTop: 22,
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
    paddingHorizontal: 12,
  },
});
