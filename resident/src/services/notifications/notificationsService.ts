import { apiClient } from '@/services/api/client';

export type NotificationType = 'BillIssued' | 'BillOverdue' | 'PaymentConfirmed' | 'DueReminder' | string;

export type AppNotification = {
  notificationId: number;
  type: NotificationType;
  title: string;
  body: string;
  deepLink: string | null;
  isRead: boolean;
  sentAt: string;
};

export async function getNotifications(): Promise<AppNotification[]> {
  const { data } = await apiClient.get<AppNotification[]>('/residents/notifications');
  return data;
}

export async function markNotificationAsRead(notificationId: number): Promise<void> {
  await apiClient.put(`/residents/notifications/${notificationId}/read`);
}

export async function markAllNotificationsAsRead(): Promise<void> {
  await apiClient.put('/residents/notifications/read-all');
}

export async function registerNotificationToken(expoPushToken: string, deviceInfo: string): Promise<void> {
  await apiClient.post('/residents/notification-tokens', { expoPushToken, deviceInfo });
}
