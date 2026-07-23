import { Feather } from '@expo/vector-icons';
import { Redirect, Tabs } from 'expo-router';
import type { ComponentProps } from 'react';
import type { ColorValue } from 'react-native';

import { useAuth } from '@/services/auth/AuthContext';
import { NotificationsProvider, useNotifications } from '@/services/notifications/NotificationsContext';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

type IconName = ComponentProps<typeof Feather>['name'];

function tabIcon(name: IconName) {
  return ({ color, size }: { color: ColorValue; size: number }) => (
    <Feather name={name} color={color as string} size={size} />
  );
}

function TabsNavigator() {
  const { colors } = useTheme();
  const { unreadCount } = useNotifications();

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: colors.accent,
        tabBarInactiveTintColor: colors.textSecondary,
        tabBarStyle: {
          backgroundColor: colors.surface,
          borderTopColor: colors.border,
        },
        tabBarLabelStyle: {
          fontFamily: fonts.bodyMedium,
          fontSize: 11,
        },
        tabBarBadgeStyle: {
          backgroundColor: colors.danger,
          fontFamily: fonts.bodySemiBold,
          fontSize: 10,
        },
      }}
    >
      <Tabs.Screen name="home" options={{ title: 'Home', tabBarIcon: tabIcon('home') }} />
      <Tabs.Screen name="bills" options={{ title: 'Bills', tabBarIcon: tabIcon('file-text') }} />
      <Tabs.Screen name="pay" options={{ title: 'Pay', tabBarIcon: tabIcon('plus-circle') }} />
      <Tabs.Screen
        name="alerts"
        options={{
          title: 'Alerts',
          tabBarIcon: tabIcon('bell'),
          tabBarBadge: unreadCount > 0 ? unreadCount : undefined,
        }}
      />
      <Tabs.Screen name="profile" options={{ title: 'Profile', tabBarIcon: tabIcon('user') }} />
    </Tabs>
  );
}

export default function TabsLayout() {
  const { resident, isLoading } = useAuth();

  if (isLoading) {
    return null;
  }

  if (!resident) {
    return <Redirect href="/(auth)/login" />;
  }

  return (
    <NotificationsProvider>
      <TabsNavigator />
    </NotificationsProvider>
  );
}
