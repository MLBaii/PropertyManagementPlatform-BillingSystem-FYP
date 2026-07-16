import React from 'react';
import { Pressable, ScrollView, StyleSheet, Text } from 'react-native';

import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

export type DisputeFilter = 'All' | 'Open' | 'UnderReview' | 'Resolved' | 'Rejected';

const FILTERS: DisputeFilter[] = ['All', 'Open', 'UnderReview', 'Resolved', 'Rejected'];

export const DISPUTE_FILTER_LABELS: Record<DisputeFilter, string> = {
  All: 'All',
  Open: 'Open',
  UnderReview: 'Under Review',
  Resolved: 'Resolved',
  Rejected: 'Rejected',
};

type Props = {
  value: DisputeFilter;
  onChange: (filter: DisputeFilter) => void;
};

export function DisputeFilterRow({ value, onChange }: Props) {
  return (
    <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.row}>
      {FILTERS.map((filter) => {
        const isActive = filter === value;
        return (
          <Pressable
            key={filter}
            onPress={() => onChange(filter)}
            style={[styles.pill, isActive && styles.pillActive]}
          >
            <Text style={[styles.label, isActive && styles.labelActive]}>
              {DISPUTE_FILTER_LABELS[filter]}
            </Text>
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
