import { router, useFocusEffect } from 'expo-router';
import React, { useCallback } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';

import { NotificationRow } from '@/components/alerts/NotificationRow';
import { Card } from '@/components/ui/Card';
import { Screen } from '@/components/ui/Screen';
import { useNotifications } from '@/services/notifications/NotificationsContext';
import { AppNotification } from '@/services/notifications/notificationsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

export default function AlertsScreen() {
  const { notifications, unreadCount, isLoading, loadError, refresh, markAsRead, markAllAsRead } =
    useNotifications();

  // Tab screens stay mounted when you switch away — refetch on every focus so a
  // notification triggered while this tab wasn't active (or a read made elsewhere) shows up
  // without needing an app reload, same fix as the Bills tab's earlier staleness bug.
  useFocusEffect(
    useCallback(() => {
      void refresh();
    }, [refresh])
  );

  const handlePress = (notification: AppNotification) => {
    if (!notification.isRead) {
      void markAsRead(notification.notificationId);
    }
    if (notification.deepLink) {
      // deepLink is an arbitrary string from the backend, not a route Expo Router's typed
      // routes can verify statically — the cast is inherent to that, not a workaround for a bug.
      router.push(notification.deepLink as Parameters<typeof router.push>[0]);
    }
  };

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.header}>
          <View>
            <Text style={styles.eyebrow}>{unreadCount} unread</Text>
            <Text style={styles.title}>Notifications</Text>
          </View>
          <Text
            style={[styles.markAllLink, unreadCount === 0 && styles.markAllLinkDisabled]}
            onPress={unreadCount > 0 ? () => void markAllAsRead() : undefined}
          >
            Mark All as Read
          </Text>
        </View>

        {isLoading ? (
          <View style={styles.centered}>
            <ActivityIndicator color={colors.accent} />
          </View>
        ) : loadError ? (
          <View style={styles.centered}>
            <Text style={styles.errorText}>{loadError}</Text>
            <Text style={styles.retryLink} onPress={() => void refresh()}>
              Try again
            </Text>
          </View>
        ) : notifications.length === 0 ? (
          <View style={styles.centered}>
            <Text style={styles.emptyText}>You have no notifications yet.</Text>
          </View>
        ) : (
          <Card style={styles.listCard}>
            {notifications.map((notification, index) => (
              <NotificationRow
                key={notification.notificationId}
                notification={notification}
                onPress={() => handlePress(notification)}
                isLast={index === notifications.length - 1}
              />
            ))}
          </Card>
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
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
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
    fontSize: 26,
    letterSpacing: -0.52,
    color: colors.text,
    marginTop: 2,
  },
  markAllLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 12,
    color: colors.accent,
  },
  markAllLinkDisabled: {
    color: colors.textSecondary,
    opacity: 0.6,
  },
  listCard: {
    paddingVertical: 2,
    paddingHorizontal: 16,
  },
  centered: {
    alignItems: 'center',
    paddingTop: 40,
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
