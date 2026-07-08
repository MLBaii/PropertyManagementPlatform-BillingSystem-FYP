import { apiClient } from '@/services/api/client';

export type NotificationPreferences = {
  pushEnabled: boolean;
  emailEnabled: boolean;
  billDueReminders: boolean;
};

export type Profile = {
  name: string;
  email: string;
  phoneNumber: string | null;
  notificationPreferences: NotificationPreferences;
  unitNumber: string;
  floor: number;
  propertyName: string;
};

export type UpdateProfilePayload = {
  name: string;
  phoneNumber: string | null;
  email: string;
};

export async function getProfile(): Promise<Profile> {
  const { data } = await apiClient.get<Profile>('/residents/profile');
  return data;
}

export async function updateProfile(
  payload: UpdateProfilePayload
): Promise<{ profile: Profile; message: string }> {
  const { data } = await apiClient.put<{ profile: Profile; message: string }>(
    '/residents/profile',
    payload
  );
  return data;
}

export async function changePassword(currentPassword: string, newPassword: string): Promise<{ message: string }> {
  const { data } = await apiClient.put<{ message: string }>('/residents/profile/password', {
    currentPassword,
    newPassword,
  });
  return data;
}

export async function updateNotificationPreferences(
  preferences: NotificationPreferences
): Promise<{ notificationPreferences: NotificationPreferences; message: string }> {
  const { data } = await apiClient.put<{ notificationPreferences: NotificationPreferences; message: string }>(
    '/residents/profile/notifications',
    preferences
  );
  return data;
}
