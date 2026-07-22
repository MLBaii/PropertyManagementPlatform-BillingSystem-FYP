import { Feather } from '@expo/vector-icons';
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { BillTagRow } from '@/components/pay/BillTagRow';
import { ProofStatusBadge } from '@/components/pay/ProofStatusBadge';
import { SourceButton } from '@/components/pay/SourceButton';
import { Card } from '@/components/ui/Card';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { Bill, getBills } from '@/services/bills/billsService';
import { PaymentProof, submitPaymentProof } from '@/services/paymentProofs/paymentProofService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { formatBillingPeriod } from '@/utils/formatDate';
import { formatFileSize } from '@/utils/formatFileSize';
import {
  isValidProofFile,
  PickedFile,
  pickFromCamera,
  pickFromFiles,
  pickFromLibrary,
  PROOF_FILE_ERROR_MESSAGE,
} from '@/utils/proofFilePicker';

type Step = 1 | 2 | 'success';
type FileSource = 'camera' | 'library' | 'files';
type PickedFileEntry = { file: PickedFile; source: FileSource };

const MAX_FILES = 3;

export default function UploadProofScreen() {
  const { billId } = useLocalSearchParams<{ billId?: string }>();
  const [step, setStep] = useState<Step>(1);

  const [files, setFiles] = useState<PickedFileEntry[]>([]);
  const [fileError, setFileError] = useState<string | undefined>();
  // Derived from the current file list, not "whatever was picked last" — a source's tick
  // must disappear the moment its last file is removed, not linger until another pick.
  const tickedSources = useMemo(() => new Set(files.map((f) => f.source)), [files]);

  const [bills, setBills] = useState<Bill[]>([]);
  const [isLoadingBills, setIsLoadingBills] = useState(true);
  const [billsError, setBillsError] = useState<string | undefined>();
  const [selectedBillIds, setSelectedBillIds] = useState<number[]>([]);
  const [hasPreTagged, setHasPreTagged] = useState(false);

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | undefined>();
  const [submittedProof, setSubmittedProof] = useState<PaymentProof | null>(null);

  // Taggable = the bills that could plausibly still need a payment: not already Paid, and
  // not already sitting on another Pending proof (ProofSubmitted). useFocusEffect (not
  // plain useEffect) so a bill that became ProofSubmitted from an earlier tagging in this
  // same session — e.g. the resident backs out and reopens this screen — is re-excluded on
  // return rather than shown stale from a mount that predates that submission.
  useFocusEffect(
    useCallback(() => {
      setIsLoadingBills(true);
      getBills()
        .then((allBills) => {
          setBills(allBills.filter((b) => b.status === 'Unpaid' || b.status === 'Overdue'));
        })
        .catch(() => setBillsError('Could not load your bills. Please try again.'))
        .finally(() => setIsLoadingBills(false));
    }, [])
  );

  // Pre-tags the bill the resident arrived from (Bill Detail's "Upload Payment Proof" button),
  // once, the first time the taggable list includes it — a no-op if it's already settled/tagged.
  useEffect(() => {
    if (hasPreTagged || bills.length === 0 || !billId) {
      return;
    }
    const numericBillId = Number(billId);
    if (bills.some((b) => b.billId === numericBillId)) {
      setSelectedBillIds((prev) => (prev.includes(numericBillId) ? prev : [...prev, numericBillId]));
    }
    setHasPreTagged(true);
  }, [bills, billId, hasPreTagged]);

  const taggedBills = useMemo(
    () => bills.filter((b) => selectedBillIds.includes(b.billId)),
    [bills, selectedBillIds]
  );
  const totalAmount = useMemo(
    () => taggedBills.reduce((sum, b) => sum + b.outstandingBalance, 0),
    [taggedBills]
  );

  const toggleBill = (id: number) => {
    setSelectedBillIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
  };

  const atFileLimit = files.length >= MAX_FILES;

  const handlePick = async (source: () => Promise<PickedFile | null>, sourceKey: FileSource) => {
    setFileError(undefined);
    if (atFileLimit) {
      setFileError(`You can upload up to ${MAX_FILES} files.`);
      return;
    }
    try {
      const picked = await source();
      if (!picked) {
        return;
      }
      if (!isValidProofFile(picked)) {
        setFileError(PROOF_FILE_ERROR_MESSAGE);
        return;
      }
      setFiles((prev) => [...prev, { file: picked, source: sourceKey }]);
    } catch (err) {
      setFileError(err instanceof Error ? err.message : 'Could not select a file. Please try again.');
    }
  };

  const removeFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
    setFileError(undefined);
  };

  const canContinue = files.length > 0 && selectedBillIds.length > 0;

  const handleSubmit = async () => {
    if (files.length === 0) {
      return;
    }
    setSubmitError(undefined);
    setIsSubmitting(true);
    try {
      const proof = await submitPaymentProof(files.map((f) => f.file), selectedBillIds);
      setSubmittedProof(proof);
      setStep('success');
    } catch {
      setSubmitError('Could not submit your payment proof. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (step === 'success' && submittedProof) {
    return (
      <Screen style={styles.centeredScreen}>
        <View style={styles.successIcon}>
          <Feather name="check" size={28} color={colors.success} />
        </View>
        <Text style={styles.successTitle}>Proof Submitted</Text>
        <ProofStatusBadge status={submittedProof.status} />
        <Text style={styles.successNote}>You'll be notified once the admin verifies it.</Text>
        <View style={styles.successButtonWrap}>
          <PrimaryButton label="Done" onPress={() => router.replace('/(tabs)/pay')} />
        </View>
      </Screen>
    );
  }

  const renderFileRow = (entry: PickedFileEntry, index: number, removable: boolean) => (
    <Card key={`${entry.file.uri}-${index}`} style={styles.previewCard}>
      <View style={styles.previewIcon}>
        <Feather
          name={entry.file.mimeType === 'application/pdf' ? 'file-text' : 'image'}
          size={18}
          color={colors.accent}
        />
      </View>
      <View style={styles.previewTextCol}>
        <Text style={styles.previewName} numberOfLines={1}>
          {entry.file.name}
        </Text>
        <Text style={styles.previewMeta}>{entry.file.size !== null ? formatFileSize(entry.file.size) : ''}</Text>
      </View>
      {removable && (
        <Pressable onPress={() => removeFile(index)} hitSlop={8} style={styles.removeButton}>
          <Feather name="x" size={16} color={colors.textSecondary} />
        </Pressable>
      )}
    </Card>
  );

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.header}>
          <Text style={styles.eyebrow}>Step {step === 1 ? '1' : '2'} of 2</Text>
          <Text style={styles.title}>{step === 1 ? 'Upload Proof' : 'Confirm & Submit'}</Text>
        </View>

        {step === 1 ? (
          <>
            <View style={styles.sourceRow}>
              <SourceButton
                icon="camera"
                label="Camera"
                selected={tickedSources.has('camera')}
                disabled={atFileLimit}
                onPress={() => handlePick(pickFromCamera, 'camera')}
              />
              <SourceButton
                icon="image"
                label="Photo Library"
                selected={tickedSources.has('library')}
                disabled={atFileLimit}
                onPress={() => handlePick(pickFromLibrary, 'library')}
              />
              <SourceButton
                icon="file"
                label="Files"
                selected={tickedSources.has('files')}
                disabled={atFileLimit}
                onPress={() => handlePick(pickFromFiles, 'files')}
              />
            </View>

            {files.length > 0 && (
              <View style={styles.fileList}>{files.map((file, index) => renderFileRow(file, index, true))}</View>
            )}

            {fileError && <Text style={styles.errorText}>{fileError}</Text>}
            {atFileLimit && !fileError && (
              <Text style={styles.limitNote}>
                Maximum of {MAX_FILES} files reached — remove one to add another.
              </Text>
            )}

            <Text style={styles.sectionEyebrow}>Tag to bill(s)</Text>
            {isLoadingBills ? (
              <View style={styles.centered}>
                <ActivityIndicator color={colors.accent} />
              </View>
            ) : billsError ? (
              <Text style={styles.errorText}>{billsError}</Text>
            ) : bills.length === 0 ? (
              <Text style={styles.emptyText}>You have no outstanding bills to tag right now.</Text>
            ) : (
              bills.map((bill) => (
                <BillTagRow
                  key={bill.billId}
                  referenceNumber={bill.referenceNumber}
                  billingPeriod={bill.billingPeriod}
                  amount={bill.outstandingBalance}
                  checked={selectedBillIds.includes(bill.billId)}
                  onToggle={() => toggleBill(bill.billId)}
                />
              ))
            )}

            <View style={styles.continueWrap}>
              <PrimaryButton label="Continue" onPress={() => setStep(2)} disabled={!canContinue} />
            </View>
          </>
        ) : (
          <>
            <View style={styles.fileList}>{files.map((file, index) => renderFileRow(file, index, false))}</View>

            <Text style={styles.sectionEyebrow}>Tagged bills</Text>
            <Card style={styles.summaryCard}>
              {taggedBills.map((bill, index) => (
                <View
                  key={bill.billId}
                  style={[styles.summaryRow, index < taggedBills.length - 1 && styles.summaryRowBorder]}
                >
                  <View>
                    <Text style={styles.summaryReference}>{bill.referenceNumber}</Text>
                    <Text style={styles.summaryPeriod}>{formatBillingPeriod(bill.billingPeriod)}</Text>
                  </View>
                  <Text style={styles.summaryAmount}>RM {bill.outstandingBalance.toFixed(2)}</Text>
                </View>
              ))}
              <View style={[styles.summaryRow, styles.totalRow]}>
                <Text style={styles.totalLabel}>Total</Text>
                <Text style={styles.totalAmount}>RM {totalAmount.toFixed(2)}</Text>
              </View>
            </Card>

            {submitError && <Text style={styles.errorText}>{submitError}</Text>}

            <View style={styles.continueWrap}>
              <PrimaryButton label="Submit Proof" onPress={handleSubmit} loading={isSubmitting} />
              <Text style={styles.backLink} onPress={() => setStep(1)}>
                Back
              </Text>
            </View>
          </>
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
    marginBottom: 16,
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
    fontSize: 24,
    letterSpacing: -0.48,
    color: colors.text,
    marginTop: 2,
  },
  sourceRow: {
    flexDirection: 'row',
    gap: 10,
    marginBottom: 16,
  },
  fileList: {
    marginBottom: 8,
  },
  previewCard: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    marginBottom: 10,
  },
  previewIcon: {
    width: 44,
    height: 44,
    borderRadius: 10,
    backgroundColor: colors.surface2,
    alignItems: 'center',
    justifyContent: 'center',
  },
  previewTextCol: {
    flex: 1,
  },
  previewName: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.text,
  },
  previewMeta: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    marginTop: 2,
  },
  removeButton: {
    width: 28,
    height: 28,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
  },
  limitNote: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    marginBottom: 12,
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
    paddingVertical: 16,
  },
  errorText: {
    fontFamily: fonts.body,
    fontSize: 12,
    color: colors.danger,
    marginBottom: 12,
  },
  emptyText: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    marginBottom: 12,
  },
  continueWrap: {
    marginTop: 8,
    gap: 14,
  },
  backLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: 'center',
  },
  summaryCard: {
    marginBottom: 18,
  },
  summaryRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 11,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  summaryRowBorder: {
    borderBottomWidth: 1,
  },
  summaryReference: {
    fontFamily: fonts.mono,
    fontSize: 12,
    color: colors.text,
  },
  summaryPeriod: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    marginTop: 2,
  },
  summaryAmount: {
    fontFamily: fonts.mono,
    fontSize: 13,
    color: colors.text,
  },
  totalRow: {
    borderBottomWidth: 0,
    paddingTop: 13,
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
  centeredScreen: {
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
  },
  successIcon: {
    width: 64,
    height: 64,
    borderRadius: 32,
    backgroundColor: colors.successBg,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 6,
  },
  successTitle: {
    fontFamily: fonts.heading,
    fontSize: 22,
    color: colors.text,
  },
  successNote: {
    fontFamily: fonts.body,
    fontSize: 12,
    color: colors.textSecondary,
    textAlign: 'center',
    marginTop: 4,
    marginBottom: 12,
    paddingHorizontal: 24,
  },
  successButtonWrap: {
    width: '100%',
    paddingHorizontal: 24,
  },
});
