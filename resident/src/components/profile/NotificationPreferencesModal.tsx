import React, { useEffect, useState } from 'react';
import { StyleSheet, Switch, Text, View } from 'react-native';

import { ModalSheet } from '@/components/ui/ModalSheet';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { NotificationPreferences, updateNotificationPreferences } from '@/services/profile/profileService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  visible: boolean;
  onClose: () => void;
  preferences: NotificationPreferences;
  onSaved: (preferences: NotificationPreferences) => void;
};

const TOGGLES: { key: keyof NotificationPreferences; label: string }[] = [
  { key: 'pushEnabled', label: 'Push notifications' },
  { key: 'emailEnabled', label: 'Email notifications' },
  { key: 'billDueReminders', label: 'Bill due reminders' },
];

export function NotificationPreferencesModal({ visible, onClose, preferences, onSaved }: Props) {
  const [draft, setDraft] = useState<NotificationPreferences>(preferences);
  const [successMessage, setSuccessMessage] = useState<string | undefined>();
  const [errorMessage, setErrorMessage] = useState<string | undefined>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (visible) {
      setDraft(preferences);
      setSuccessMessage(undefined);
      setErrorMessage(undefined);
    }
  }, [visible, preferences]);

  const handleSave = async () => {
    setSuccessMessage(undefined);
    setErrorMessage(undefined);
    setIsSubmitting(true);
    try {
      const result = await updateNotificationPreferences(draft);
      setSuccessMessage(result.message);
      onSaved(result.notificationPreferences);
    } catch {
      setErrorMessage('Could not save preferences. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ModalSheet visible={visible} title="Notification Preferences" onClose={onClose}>
      {successMessage ? <Text style={styles.success}>{successMessage}</Text> : null}
      {errorMessage ? <Text style={styles.error}>{errorMessage}</Text> : null}
      {TOGGLES.map((toggle) => (
        <View key={toggle.key} style={styles.row}>
          <Text style={styles.label}>{toggle.label}</Text>
          <Switch
            value={draft[toggle.key]}
            onValueChange={(value) => setDraft((prev) => ({ ...prev, [toggle.key]: value }))}
            trackColor={{ false: colors.border, true: colors.accentLine }}
            thumbColor={draft[toggle.key] ? colors.accent : colors.textSecondary}
          />
        </View>
      ))}
      <PrimaryButton label="Save Preferences" onPress={handleSave} loading={isSubmitting} />
    </ModalSheet>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  label: {
    fontFamily: fonts.body,
    fontSize: 14,
    color: colors.text,
  },
  success: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.success,
    marginBottom: 14,
  },
  error: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.danger,
    marginBottom: 14,
  },
});
