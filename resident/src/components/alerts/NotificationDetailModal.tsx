import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { GhostButton } from '@/components/ui/GhostButton';
import { ModalSheet } from '@/components/ui/ModalSheet';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { AppNotification } from '@/services/notifications/notificationsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { formatRelativeTime } from '@/utils/formatDate';

type Props = {
  notification: AppNotification | null;
  onMarkAsRead: (notificationId: number) => void;
  onClose: () => void;
};

export function NotificationDetailModal({ notification, onMarkAsRead, onClose }: Props) {
  return (
    <ModalSheet visible={notification !== null} title={notification?.title ?? ''} onClose={onClose}>
      {notification && (
        <>
          <Text style={styles.timestamp}>{formatRelativeTime(notification.sentAt)}</Text>
          <Text style={styles.body}>{notification.body}</Text>
          <View style={styles.actions}>
            {!notification.isRead && (
              <PrimaryButton label="Mark as Read" onPress={() => onMarkAsRead(notification.notificationId)} />
            )}
            <GhostButton label="Close" onPress={onClose} />
          </View>
        </>
      )}
    </ModalSheet>
  );
}

const styles = StyleSheet.create({
  timestamp: {
    fontFamily: fonts.mono,
    fontSize: 11,
    color: colors.textSecondary,
    marginBottom: 10,
  },
  body: {
    fontFamily: fonts.body,
    fontSize: 14,
    lineHeight: 20,
    color: colors.text,
    marginBottom: 20,
  },
  actions: {
    gap: 10,
  },
});
