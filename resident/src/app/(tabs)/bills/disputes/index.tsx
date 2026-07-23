import { router, useFocusEffect } from 'expo-router';
import React, { useCallback, useMemo, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';

import { DisputeFilter, DisputeFilterRow, DISPUTE_FILTER_LABELS } from '@/components/disputes/DisputeFilterRow';
import { DisputeRow } from '@/components/disputes/DisputeRow';
import { Screen } from '@/components/ui/Screen';
import { Dispute, getDisputes } from '@/services/disputes/disputesService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

export default function DisputeHistoryScreen() {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const [disputes, setDisputes] = useState<Dispute[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();
  const [filter, setFilter] = useState<DisputeFilter>('All');

  // Refetches on every focus, not just first mount — same fix pattern as Bills/Alerts, so a
  // dispute submitted moments ago (or resolved by an admin) never shows stale here.
  useFocusEffect(
    useCallback(() => {
      getDisputes()
        .then((fresh) => {
          setDisputes(fresh);
          setLoadError(undefined);
        })
        .catch(() => setLoadError('Could not load your disputes. Please try again.'))
        .finally(() => setIsLoading(false));
    }, [])
  );

  const filteredDisputes = useMemo(() => {
    if (filter === 'All') {
      return disputes;
    }
    return disputes.filter((d) => d.status === filter);
  }, [disputes, filter]);

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.header}>
          <Text style={styles.backLink} onPress={() => router.back()}>
            ‹ Bills
          </Text>
          <Text style={styles.title}>Dispute History</Text>
        </View>

        <View style={styles.filterWrap}>
          <DisputeFilterRow value={filter} onChange={setFilter} />
        </View>

        {isLoading ? (
          <View style={styles.centered}>
            <ActivityIndicator color={colors.accent} />
          </View>
        ) : loadError ? (
          <View style={styles.centered}>
            <Text style={styles.errorText}>{loadError}</Text>
          </View>
        ) : disputes.length === 0 ? (
          <View style={styles.centered}>
            <Text style={styles.emptyText}>You have not submitted any disputes yet.</Text>
          </View>
        ) : filteredDisputes.length === 0 ? (
          <View style={styles.centered}>
            <Text style={styles.emptyText}>
              No {DISPUTE_FILTER_LABELS[filter].toLowerCase()} disputes.
            </Text>
          </View>
        ) : (
          filteredDisputes.map((dispute) => (
            <DisputeRow
              key={dispute.disputeId}
              dispute={dispute}
              onPress={() =>
                router.push({
                  pathname: '/(tabs)/bills/disputes/[disputeId]',
                  params: { disputeId: String(dispute.disputeId) },
                })
              }
            />
          ))
        )}
      </ScrollView>
    </Screen>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
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
    filterWrap: {
      marginBottom: 16,
    },
    centered: {
      alignItems: 'center',
      paddingTop: 40,
    },
    errorText: {
      fontFamily: fonts.body,
      fontSize: 13,
      color: colors.textSecondary,
      textAlign: 'center',
    },
    emptyText: {
      fontFamily: fonts.body,
      fontSize: 13,
      color: colors.textSecondary,
      textAlign: 'center',
    },
  });
