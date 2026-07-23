import { router, useFocusEffect } from 'expo-router';
import React, { useCallback, useMemo, useRef, useState } from 'react';
import { ActivityIndicator, FlatList, StyleSheet, Text, View } from 'react-native';

import { BillCard } from '@/components/bills/BillCard';
import { BillFilter, BillFilterRow, FILTER_LABELS } from '@/components/bills/BillFilterRow';
import { Screen } from '@/components/ui/Screen';
import { Bill, getBills } from '@/services/bills/billsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

export default function BillsListScreen() {
  const [bills, setBills] = useState<Bill[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();
  const [filter, setFilter] = useState<BillFilter>('All');
  const hasLoadedOnce = useRef(false);

  // Tab screens stay mounted when you switch away (e.g. to Pay to submit a proof), so a
  // plain mount-only fetch would keep showing this list's pre-submission snapshot forever —
  // refetch every time the tab regains focus, not just once. Silent after the first load
  // (no full-screen spinner) so switching back to an already-loaded list doesn't flash empty.
  useFocusEffect(
    useCallback(() => {
      if (!hasLoadedOnce.current) {
        hasLoadedOnce.current = true;
        getBills()
          .then(setBills)
          .catch(() => setLoadError('Could not load your bills. Pull down to try again.'))
          .finally(() => setIsLoading(false));
        return;
      }

      getBills()
        .then((fresh) => {
          setBills(fresh);
          setLoadError(undefined);
        })
        .catch(() => setLoadError('Could not load your bills. Pull down to try again.'));
    }, [])
  );

  // "Disputed"/"PendingDispute" aren't values of bill.status anymore (that's purely payment
  // status) — they live on the separate activeDisputeStatus field, so those two pills match
  // against that instead, regardless of the bill's underlying payment status.
  const filteredBills = useMemo(() => {
    if (filter === 'All') {
      return bills;
    }
    if (filter === 'Disputed' || filter === 'PendingDispute') {
      return bills.filter((bill) => bill.activeDisputeStatus === filter);
    }
    return bills.filter((bill) => bill.status === filter);
  }, [bills, filter]);

  if (isLoading) {
    return (
      <Screen style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </Screen>
    );
  }

  if (loadError) {
    return (
      <Screen style={styles.centered}>
        <Text style={styles.loadError}>{loadError}</Text>
      </Screen>
    );
  }

  return (
    <Screen>
      <FlatList
        data={filteredBills}
        keyExtractor={(bill) => String(bill.billId)}
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.listContent}
        ListHeaderComponent={
          <View>
            <View style={styles.header}>
              <View>
                <Text style={styles.eyebrow}>{bills.length} bills</Text>
                <Text style={styles.title}>Bills</Text>
              </View>
              <Text style={styles.disputeHistoryLink} onPress={() => router.push('/(tabs)/bills/disputes')}>
                Dispute History
              </Text>
            </View>
            <View style={styles.filterWrap}>
              <BillFilterRow value={filter} onChange={setFilter} />
            </View>
          </View>
        }
        renderItem={({ item }) => (
          <BillCard
            bill={item}
            onPress={() =>
              router.push({ pathname: '/(tabs)/bills/[billId]', params: { billId: String(item.billId) } })
            }
          />
        )}
        ListEmptyComponent={
          <View style={styles.emptyState}>
            <Text style={styles.emptyText}>
              {bills.length === 0
                ? 'You have no bills yet — check back after your first billing cycle.'
                : `No ${FILTER_LABELS[filter].toLowerCase()} bills.`}
            </Text>
          </View>
        }
      />
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
  listContent: {
    paddingTop: 10,
    paddingBottom: 32,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'flex-end',
    justifyContent: 'space-between',
    marginVertical: 4,
    marginBottom: 14,
  },
  disputeHistoryLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 12,
    color: colors.accent,
    marginBottom: 3,
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
  filterWrap: {
    marginBottom: 16,
  },
  emptyState: {
    paddingTop: 40,
    paddingHorizontal: 12,
  },
  emptyText: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: 'center',
  },
});
