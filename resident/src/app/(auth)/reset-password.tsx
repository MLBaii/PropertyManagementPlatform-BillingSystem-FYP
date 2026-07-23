import { Feather } from '@expo/vector-icons';
import axios from 'axios';
import { router } from 'expo-router';
import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { TextField } from '@/components/ui/TextField';
import { resetPassword } from '@/services/auth/authService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

const MIN_PASSWORD_LENGTH = 8;

export default function ResetPasswordScreen() {
  const { colors } = useTheme();
  const styles = createStyles(colors);

  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [tokenError, setTokenError] = useState<string | undefined>();
  const [passwordError, setPasswordError] = useState<string | undefined>();
  const [confirmError, setConfirmError] = useState<string | undefined>();
  const [formError, setFormError] = useState<string | undefined>();

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);

  const handleSubmit = async () => {
    setFormError(undefined);

    let hasError = false;
    if (!token.trim()) {
      setTokenError('Reset code is required.');
      hasError = true;
    } else {
      setTokenError(undefined);
    }

    if (newPassword.length < MIN_PASSWORD_LENGTH) {
      setPasswordError('Password must be at least 8 characters long.');
      hasError = true;
    } else {
      setPasswordError(undefined);
    }

    if (confirmPassword !== newPassword) {
      setConfirmError('Passwords do not match. Please re-enter.');
      hasError = true;
    } else {
      setConfirmError(undefined);
    }

    if (hasError) {
      return;
    }

    setIsSubmitting(true);
    try {
      await resetPassword(token.trim(), newPassword);
      setIsDone(true);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 400) {
        const message = (err.response.data as { message?: string } | undefined)?.message;
        setFormError(message ?? 'This reset code is invalid or has expired. Please request a new one.');
      } else {
        setFormError('Could not reach the server. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isDone) {
    return (
      <Screen style={styles.centeredScreen}>
        <View style={styles.successIcon}>
          <Feather name="check" size={28} color={colors.success} />
        </View>
        <Text style={styles.successTitle}>Password Reset</Text>
        <Text style={styles.successNote}>
          Your password has been reset successfully. Please log in with your new password.
        </Text>
        <View style={styles.successButtonWrap}>
          <PrimaryButton label="Go to Login" onPress={() => router.replace('/(auth)/login')} />
        </View>
      </Screen>
    );
  }

  return (
    <Screen>
      <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <ScrollView contentContainerStyle={styles.scrollContent} keyboardShouldPersistTaps="handled">
          <View style={styles.header}>
            <Pressable style={styles.backRow} onPress={() => router.back()} hitSlop={10}>
              <Text style={styles.backText}>‹ Back</Text>
            </Pressable>
            <Text style={styles.title}>Enter Reset Code</Text>
            <Text style={styles.subtitle}>
              Enter the code from your email and choose a new password.
            </Text>
          </View>

          <TextField
            label="Reset code"
            placeholder="Paste the code from your email"
            autoCapitalize="none"
            autoCorrect={false}
            value={token}
            onChangeText={setToken}
            error={tokenError}
          />
          <TextField
            label="New password"
            isPassword
            autoCapitalize="none"
            value={newPassword}
            onChangeText={setNewPassword}
            error={passwordError}
          />
          <TextField
            label="Confirm new password"
            isPassword
            autoCapitalize="none"
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            error={confirmError ?? formError}
          />

          <PrimaryButton label="Reset Password" onPress={handleSubmit} loading={isSubmitting} />
        </ScrollView>
      </KeyboardAvoidingView>
    </Screen>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    flex: {
      flex: 1,
    },
    scrollContent: {
      flexGrow: 1,
      paddingVertical: 24,
    },
    header: {
      marginBottom: 22,
    },
    backRow: {
      marginBottom: 16,
    },
    backText: {
      fontFamily: fonts.bodyMedium,
      fontSize: 13,
      color: colors.accent,
    },
    title: {
      fontFamily: fonts.heading,
      fontSize: 26,
      letterSpacing: -0.52,
      color: colors.text,
      marginBottom: 6,
    },
    subtitle: {
      fontFamily: fonts.body,
      fontSize: 13,
      color: colors.textSecondary,
      lineHeight: 18,
    },
    centeredScreen: {
      alignItems: 'center',
      justifyContent: 'center',
      gap: 10,
    },
    successIcon: {
      width: 64,
      height: 64,
      borderRadius: 32,
      backgroundColor: colors.successBg,
      alignItems: 'center',
      justifyContent: 'center',
      marginBottom: 6,
    },
    successTitle: {
      fontFamily: fonts.heading,
      fontSize: 22,
      color: colors.text,
    },
    successNote: {
      fontFamily: fonts.body,
      fontSize: 12,
      color: colors.textSecondary,
      textAlign: 'center',
      marginTop: 4,
      marginBottom: 12,
      paddingHorizontal: 24,
    },
    successButtonWrap: {
      width: '100%',
      paddingHorizontal: 24,
    },
  });
