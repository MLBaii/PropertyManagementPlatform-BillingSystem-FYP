import React from 'react';
import { Pressable, ScrollView, StyleSheet, Text } from 'react-native';

import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

export type BillFilter =
  | 'All'
  | 'Unpaid'
  | 'Overdue'
  | 'ProofSubmitted'
  | 'Disputed'
  | 'PendingDispute'
  | 'Paid';

const FILTERS: BillFilter[] = [
  'All',
  'Unpaid',
  'Overdue',
  'ProofSubmitted',
  'Disputed',
  'PendingDispute',
  'Paid',
];

export const FILTER_LABELS: Record<BillFilter, string> = {
  All: 'All',
  Unpaid: 'Unpaid',
  Overdue: 'Overdue',
  ProofSubmitted: 'Proof Submitted',
  Disputed: 'Disputed',
  PendingDispute: 'Pending Dispute',
  Paid: 'Paid',
};

type Props = {
  value: BillFilter;
  onChange: (filter: BillFilter) => void;
};

export function BillFilterRow({ value, onChange }: Props) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.row}
    >
      {FILTERS.map((filter) => {
        const isActive = filter === value;
        return (
          <Pressable
            key={filter}
            onPress={() => onChange(filter)}
            style={[styles.pill, isActive && styles.pillActive]}
          >
            <Text style={[styles.label, isActive && styles.labelActive]}>{FILTER_LABELS[filter]}</Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  row: {
    gap: 8,
    paddingBottom: 4,
  },
  pill: {
    paddingHorizontal: 13,
    paddingVertical: 7,
    borderRadius: 20,
    backgroundColor: colors.surface2,
    borderWidth: 1,
    borderColor: colors.border,
  },
  pillActive: {
    backgroundColor: colors.accentSoft,
    borderColor: colors.accentLine,
  },
  label: {
    fontFamily: fonts.body,
    fontSize: 12,
    color: colors.textSecondary,
  },
  labelActive: {
    color: colors.accent,
    fontFamily: fonts.bodyMedium,
  },
});
