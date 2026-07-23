import { router } from 'expo-router';
import React, { useCallback, useEffect, useState } from 'react';
import { ActivityIndicator, RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';

import { ActivityRow } from '@/components/dashboard/ActivityRow';
import { SummaryCard } from '@/components/dashboard/SummaryCard';
import { Avatar } from '@/components/ui/Avatar';
import { Card } from '@/components/ui/Card';
import { Screen } from '@/components/ui/Screen';
import { useAuth } from '@/services/auth/AuthContext';
import { Dashboard, getDashboard } from '@/services/dashboard/dashboardService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

export default function HomeScreen() {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const { resident } = useAuth();
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [loadError, setLoadError] = useState<string | undefined>();

  const load = useCallback(() => {
    return getDashboard()
      .then((data) => {
        setDashboard(data);
        setLoadError(undefined);
      })
      .catch(() => setLoadError('Could not load your dashboard. Pull down to try again.'));
  }, []);

  useEffect(() => {
    load().finally(() => setIsLoading(false));
  }, [load]);

  const handleRefresh = () => {
    setIsRefreshing(true);
    load().finally(() => setIsRefreshing(false));
  };

  if (isLoading) {
    return (
      <Screen style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </Screen>
    );
  }

  if (!dashboard) {
    return (
      <Screen style={styles.centered}>
        <Text style={styles.loadError}>{loadError}</Text>
      </Screen>
    );
  }

  const hasOutstanding = dashboard.totalOutstanding > 0;

  return (
    <Screen>
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl refreshing={isRefreshing} onRefresh={handleRefresh} tintColor={colors.accent} />
        }
      >
        <View style={styles.header}>
          <View>
            <Text style={styles.eyebrow}>
              Unit {dashboard.unitNumber} · {dashboard.propertyName}
            </Text>
            <Text style={styles.title}>Dashboard</Text>
          </View>
          <Avatar name={resident?.fullName ?? '?'} size={42} />
        </View>

        <SummaryCard
          eyebrow="Total Outstanding"
          amount={dashboard.totalOutstanding}
          color={hasOutstanding ? colors.danger : colors.success}
          variant="primary"
          badge={
            hasOutstanding
              ? { label: 'Action needed', color: colors.danger, bg: colors.dangerBg }
              : { label: 'All settled', color: colors.success, bg: colors.successBg }
          }
        />

        <View style={styles.grid}>
          <SummaryCard eyebrow="Total Paid" amount={dashboard.totalPaid} color={colors.success} />
          <SummaryCard eyebrow="Credit Balance" amount={dashboard.creditBalance} color={colors.unpaid} />
        </View>

        <View style={styles.activityHeader}>
          <Text style={styles.eyebrow}>Recent Activity</Text>
          <Text style={styles.link} onPress={() => router.push('/(tabs)/bills')}>
            View Full History
          </Text>
        </View>

        <Card style={styles.activityCard}>
          {dashboard.recentActivity.length === 0 ? (
            <Text style={styles.emptyText}>No activity yet — charges and payments will show up here.</Text>
          ) : (
            dashboard.recentActivity.map((activity, index) => (
              <ActivityRow
                key={`${activity.type}-${activity.date}-${index}`}
                activity={activity}
                showBorder={index < dashboard.recentActivity.length - 1}
              />
            ))
          )}
        </Card>
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
    header: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
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
      marginTop: 2,
    },
    grid: {
      flexDirection: 'row',
      gap: 12,
      marginTop: 12,
      marginBottom: 22,
    },
    activityHeader: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
      marginBottom: 10,
    },
    link: {
      fontFamily: fonts.bodyMedium,
      fontSize: 12,
      color: colors.accent,
    },
    activityCard: {
      paddingHorizontal: 16,
      paddingVertical: 4,
    },
    emptyText: {
      fontFamily: fonts.body,
      fontSize: 13,
      color: colors.textSecondary,
      textAlign: 'center',
      paddingVertical: 24,
    },
  });
