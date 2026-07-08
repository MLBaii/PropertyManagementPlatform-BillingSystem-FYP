import axios from 'axios';
import React, { useEffect, useState } from 'react';
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from 'react-native';

import { ChangePasswordModal } from '@/components/profile/ChangePasswordModal';
import { NotificationPreferencesModal } from '@/components/profile/NotificationPreferencesModal';
import { Avatar } from '@/components/ui/Avatar';
import { Card } from '@/components/ui/Card';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { SettingsLinkRow } from '@/components/ui/SettingsLinkRow';
import { TextField } from '@/components/ui/TextField';
import { useAuth } from '@/services/auth/AuthContext';
import { getProfile, NotificationPreferences, Profile, updateProfile } from '@/services/profile/profileService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

function isValidEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

export default function ProfileScreen() {
  const { logout } = useAuth();

  const [profile, setProfile] = useState<Profile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | undefined>();

  const [isEditing, setIsEditing] = useState(false);
  const [name, setName] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [email, setEmail] = useState('');
  const [nameError, setNameError] = useState<string | undefined>();
  const [emailError, setEmailError] = useState<string | undefined>();
  const [isSaving, setIsSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | undefined>();

  const [isPasswordModalVisible, setIsPasswordModalVisible] = useState(false);
  const [isNotificationsModalVisible, setIsNotificationsModalVisible] = useState(false);

  useEffect(() => {
    getProfile()
      .then((data) => {
        setProfile(data);
        setName(data.name);
        setPhoneNumber(data.phoneNumber ?? '');
        setEmail(data.email);
      })
      .catch(() => setLoadError('Could not load your profile. Pull down to try again.'))
      .finally(() => setIsLoading(false));
  }, []);

  const startEditing = () => {
    setSuccessMessage(undefined);
    setIsEditing(true);
  };

  const cancelEditing = () => {
    if (profile) {
      setName(profile.name);
      setPhoneNumber(profile.phoneNumber ?? '');
      setEmail(profile.email);
    }
    setNameError(undefined);
    setEmailError(undefined);
    setIsEditing(false);
  };

  const handleSave = async () => {
    setSuccessMessage(undefined);

    let hasError = false;
    if (!name.trim()) {
      setNameError('Full name is required.');
      hasError = true;
    } else {
      setNameError(undefined);
    }

    if (!email.trim()) {
      setEmailError('Email is required.');
      hasError = true;
    } else if (!isValidEmail(email.trim())) {
      setEmailError('Enter a valid email address.');
      hasError = true;
    } else {
      setEmailError(undefined);
    }

    if (hasError) {
      return;
    }

    setIsSaving(true);
    try {
      const result = await updateProfile({
        name: name.trim(),
        phoneNumber: phoneNumber.trim() || null,
        email: email.trim(),
      });
      setProfile(result.profile);
      setSuccessMessage(result.message);
      setIsEditing(false);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 409) {
        setEmailError('That email is already in use by another account.');
      } else {
        setEmailError('Could not save your profile. Please try again.');
      }
    } finally {
      setIsSaving(false);
    }
  };

  const handlePreferencesSaved = (preferences: NotificationPreferences) => {
    setProfile((prev) => (prev ? { ...prev, notificationPreferences: preferences } : prev));
  };

  if (isLoading) {
    return (
      <Screen style={styles.centered}>
        <ActivityIndicator color={colors.accent} />
      </Screen>
    );
  }

  if (!profile) {
    return (
      <Screen style={styles.centered}>
        <Text style={styles.loadError}>{loadError}</Text>
      </Screen>
    );
  }

  return (
    <Screen>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <View style={styles.header}>
          <Avatar name={profile.name} />
          <Text style={styles.name}>{profile.name}</Text>
          <Text style={styles.unitLine}>
            Unit {profile.unitNumber} · {profile.propertyName}
          </Text>
        </View>

        <View style={styles.sectionHeaderRow}>
          <Text style={styles.eyebrow}>Contact information</Text>
          {!isEditing && (
            <Text style={styles.editLink} onPress={startEditing}>
              Edit
            </Text>
          )}
        </View>

        {successMessage ? <Text style={styles.success}>{successMessage}</Text> : null}

        <View style={styles.fieldStack}>
          <TextField
            label="Full name"
            value={name}
            onChangeText={setName}
            editable={isEditing}
            error={nameError}
          />
          <TextField
            label="Phone number"
            value={phoneNumber}
            onChangeText={setPhoneNumber}
            editable={isEditing}
            keyboardType="phone-pad"
          />
          <TextField
            label="Email"
            value={email}
            onChangeText={setEmail}
            editable={isEditing}
            autoCapitalize="none"
            keyboardType="email-address"
            error={emailError}
          />
        </View>

        {isEditing && (
          <View style={styles.editActions}>
            <PrimaryButton label="Save Changes" onPress={handleSave} loading={isSaving} />
            <Text style={styles.cancelLink} onPress={cancelEditing}>
              Cancel
            </Text>
          </View>
        )}

        <Text style={styles.eyebrowTight}>Unit details (read-only)</Text>
        <Card style={styles.unitCard}>
          <View style={styles.lineItem}>
            <Text style={styles.lineLabel}>Unit</Text>
            <Text style={styles.lineValue}>{profile.unitNumber}</Text>
          </View>
          <View style={styles.lineItem}>
            <Text style={styles.lineLabel}>Floor</Text>
            <Text style={styles.lineValue}>{profile.floor}</Text>
          </View>
          <View style={[styles.lineItem, styles.lineItemLast]}>
            <Text style={styles.lineLabel}>Property</Text>
            <Text style={styles.lineValueBody}>{profile.propertyName}</Text>
          </View>
        </Card>

        <Card style={styles.settingsCard}>
          <SettingsLinkRow label="Notification Preferences" onPress={() => setIsNotificationsModalVisible(true)} />
          <SettingsLinkRow label="Change Password" onPress={() => setIsPasswordModalVisible(true)} />
          <SettingsLinkRow label="Log Out" onPress={logout} destructive showBorder={false} />
        </Card>
      </ScrollView>

      <ChangePasswordModal visible={isPasswordModalVisible} onClose={() => setIsPasswordModalVisible(false)} />
      <NotificationPreferencesModal
        visible={isNotificationsModalVisible}
        onClose={() => setIsNotificationsModalVisible(false)}
        preferences={profile.notificationPreferences}
        onSaved={handlePreferencesSaved}
      />
    </Screen>
  );
}

const styles = StyleSheet.create({
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
    alignItems: 'center',
    marginBottom: 22,
    marginTop: 10,
  },
  name: {
    fontFamily: fonts.heading,
    fontSize: 20,
    color: colors.text,
    marginTop: 12,
  },
  unitLine: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    marginTop: 3,
  },
  sectionHeaderRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginBottom: 10,
  },
  eyebrow: {
    fontFamily: fonts.body,
    fontSize: 10,
    letterSpacing: 1.5,
    textTransform: 'uppercase',
    color: colors.textSecondary,
  },
  eyebrowTight: {
    fontFamily: fonts.body,
    fontSize: 10,
    letterSpacing: 1.5,
    textTransform: 'uppercase',
    color: colors.textSecondary,
    marginBottom: 6,
  },
  editLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.accent,
  },
  success: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.success,
    marginBottom: 12,
  },
  fieldStack: {
    marginBottom: 20,
  },
  editActions: {
    marginTop: -6,
    marginBottom: 20,
    gap: 14,
  },
  cancelLink: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.textSecondary,
    textAlign: 'center',
  },
  unitCard: {
    marginBottom: 20,
  },
  settingsCard: {
    paddingHorizontal: 16,
    paddingVertical: 0,
  },
  lineItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingVertical: 11,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  lineItemLast: {
    borderBottomWidth: 0,
  },
  lineLabel: {
    fontFamily: fonts.body,
    fontSize: 13.5,
    color: colors.textSecondary,
  },
  lineValue: {
    fontFamily: fonts.body,
    fontSize: 13.5,
    color: colors.text,
  },
  lineValueBody: {
    fontFamily: fonts.body,
    fontSize: 13.5,
    color: colors.text,
  },
});
