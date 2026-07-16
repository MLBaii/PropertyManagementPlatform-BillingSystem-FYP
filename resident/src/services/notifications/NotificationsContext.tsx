import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';

import { useAuth } from '@/services/auth/AuthContext';

import {
  AppNotification,
  getNotifications,
  markAllNotificationsAsRead,
  markNotificationAsRead,
} from './notificationsService';

type NotificationsContextValue = {
  notifications: AppNotification[];
  unreadCount: number;
  isLoading: boolean;
  loadError: string | undefined;
  refresh: () => Promise<void>;
  markAsRead: (notificationId: number) => Promise<void>;
  markAllAsRead: () => Promise<void>;
};

const NotificationsContext = createContext<NotificationsContextValue | undefined>(undefined);

// A single source of truth for both the Alerts screen's list and the tab bar's unread
// badge — if each fetched independently, marking one notification read on the Alerts screen
// wouldn't update the badge until its own next fetch, and the two could disagree exactly
// like the Bills/Pay staleness bug fixed earlier in this project.
export function NotificationsProvider({ children }: { children: React.ReactNode }) {
  const { resident } = useAuth();
  const [notifications, setNotifications] = useState<AppNotification[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();

  const refresh = useCallback(async () => {
    if (!resident) {
      return;
    }
    try {
      const fresh = await getNotifications();
      setNotifications(fresh);
      setLoadError(undefined);
    } catch {
      setLoadError('Could not load your notifications. Please try again.');
    } finally {
      setIsLoading(false);
    }
  }, [resident]);

  const markAsRead = useCallback(async (notificationId: number) => {
    setNotifications((prev) =>
      prev.map((n) => (n.notificationId === notificationId ? { ...n, isRead: true } : n))
    );
    try {
      await markNotificationAsRead(notificationId);
    } catch {
      // Best-effort optimistic update — a subsequent refresh() will reconcile if this failed.
    }
  }, []);

  const markAllAsRead = useCallback(async () => {
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
    try {
      await markAllNotificationsAsRead();
    } catch {
      // Best-effort optimistic update, same as markAsRead.
    }
  }, []);

  useEffect(() => {
    // Fetches once as soon as a resident is available (login or app-start session restore)
    // so the tab badge is correct even before the resident ever opens the Alerts tab — the
    // Alerts screen itself refetches again on every focus (see alerts.tsx) to stay current
    // after that.
    void refresh();
  }, [refresh]);

  const unreadCount = useMemo(() => notifications.filter((n) => !n.isRead).length, [notifications]);

  const value = useMemo(
    () => ({ notifications, unreadCount, isLoading, loadError, refresh, markAsRead, markAllAsRead }),
    [notifications, unreadCount, isLoading, loadError, refresh, markAsRead, markAllAsRead]
  );

  return <NotificationsContext.Provider value={value}>{children}</NotificationsContext.Provider>;
}

export function useNotifications(): NotificationsContextValue {
  const ctx = useContext(NotificationsContext);
  if (!ctx) {
    throw new Error('useNotifications must be used within a NotificationsProvider');
  }
  return ctx;
}
