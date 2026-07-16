import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import { AppNotification } from '@/services/notifications/notificationsService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';
import { formatRelativeTime } from '@/utils/formatDate';

type Props = {
  notification: AppNotification;
  onPress: () => void;
  isLast: boolean;
};

export function NotificationRow({ notification, onPress, isLast }: Props) {
  const unread = !notification.isRead;

  return (
    <Pressable
      onPress={onPress}
      style={[styles.row, !isLast && styles.rowBorder]}
    >
      <View style={[styles.dot, unread && styles.dotUnread]} />
      <View style={styles.textCol}>
        <Text style={[styles.title, unread && styles.titleUnread]}>{notification.title}</Text>
        <Text style={styles.body}>{notification.body}</Text>
        <Text style={styles.timestamp}>{formatRelativeTime(notification.sentAt)}</Text>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    gap: 12,
    paddingVertical: 13,
  },
  rowBorder: {
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: 4,
    marginTop: 6,
    flexShrink: 0,
    backgroundColor: 'transparent',
  },
  dotUnread: {
    backgroundColor: colors.accent,
  },
  textCol: {
    flex: 1,
  },
  title: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.text,
  },
  titleUnread: {
    fontFamily: fonts.bodySemiBold,
  },
  body: {
    fontFamily: fonts.body,
    fontSize: 12,
    color: colors.textSecondary,
    marginTop: 3,
    marginBottom: 4,
  },
  timestamp: {
    fontFamily: fonts.mono,
    fontSize: 10,
    color: colors.textSecondary,
  },
});
