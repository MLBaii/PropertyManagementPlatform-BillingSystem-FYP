import Constants from 'expo-constants';
import * as Device from 'expo-device';
import * as Notifications from 'expo-notifications';
import { Platform } from 'react-native';

import { registerNotificationToken } from '@/services/notifications/notificationsService';

// Foreground behavior: show the banner + play the sound even while the app is open, rather
// than silently updating the badge only (the OS default when no handler is set).
Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowBanner: true,
    shouldShowList: true,
    shouldPlaySound: true,
    shouldSetBadge: true,
  }),
});

// Called on login and on app-start session restore (see AuthContext) — registers this
// device's Expo push token with the backend so it can receive pushes. Every failure path
// here is deliberately non-blocking (per UC-101 A5's "permission denied doesn't block the
// resident from using the app"): a caught error just means no push for this device/session,
// never a thrown error the caller has to handle.
export async function registerForPushNotificationsAsync(): Promise<void> {
  try {
    if (Platform.OS === 'android') {
      await Notifications.setNotificationChannelAsync('default', {
        name: 'default',
        importance: Notifications.AndroidImportance.DEFAULT,
      });
    }

    if (!Device.isDevice) {
      // Simulators/emulators can't receive push tokens — not an error, just nothing to do.
      return;
    }

    const { status: existingStatus } = await Notifications.getPermissionsAsync();
    let finalStatus = existingStatus;
    if (existingStatus !== 'granted') {
      const { status } = await Notifications.requestPermissionsAsync();
      finalStatus = status;
    }
    if (finalStatus !== 'granted') {
      return;
    }

    const projectId = Constants.expoConfig?.extra?.eas?.projectId ?? Constants.easConfig?.projectId;
    const tokenResponse = await Notifications.getExpoPushTokenAsync(
      projectId ? { projectId } : undefined
    );

    await registerNotificationToken(
      tokenResponse.data,
      `${Platform.OS} ${Platform.Version} · ${Device.modelName ?? 'unknown device'}`
    );
  } catch (err) {
    // Expected in Expo Go on SDK 53+ (no remote push support there) and whenever there's no
    // EAS projectId configured for a dev build — both are environment limitations, not bugs,
    // so this stays a warning rather than surfacing anywhere blocking.
    console.warn('Push notification registration skipped:', err instanceof Error ? err.message : err);
  }
}
