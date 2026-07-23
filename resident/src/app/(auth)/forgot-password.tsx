import { router } from 'expo-router';
import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { PropertyContactCard } from '@/components/auth/PropertyContactCard';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { TextField } from '@/components/ui/TextField';
import { forgotPassword } from '@/services/auth/authService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

function isValidEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

// UC-101 A1. The backend always returns the same generic message whether or not the email
// matched a resident (anti-enumeration) — this screen has nothing to branch on beyond a
// network failure, so "success" here just means the request reached the server.
export default function ForgotPasswordScreen() {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const [email, setEmail] = useState('');
  const [emailError, setEmailError] = useState<string | undefined>();
  const [formError, setFormError] = useState<string | undefined>();
  const [successMessage, setSuccessMessage] = useState<string | undefined>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSend = async () => {
    setFormError(undefined);
    setSuccessMessage(undefined);

    const trimmedEmail = email.trim();
    if (!trimmedEmail) {
      setEmailError('Email is required.');
      return;
    }
    if (!isValidEmail(trimmedEmail)) {
      setEmailError('Enter a valid email address.');
      return;
    }
    setEmailError(undefined);

    setIsSubmitting(true);
    try {
      const message = await forgotPassword(trimmedEmail);
      setSuccessMessage(message);
    } catch {
      setFormError('Could not reach the server. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Screen>
      <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
        <ScrollView contentContainerStyle={styles.scrollContent} keyboardShouldPersistTaps="handled">
          <View style={styles.header}>
            <Pressable style={styles.backRow} onPress={() => router.back()} hitSlop={10}>
              <Text style={styles.backText}>‹ Login</Text>
            </Pressable>
            <Text style={styles.title}>Reset Password</Text>
            <Text style={styles.subtitle}>
              Enter your account email and we'll send you a reset code.
            </Text>
          </View>

          <TextField
            label="Email"
            placeholder="resident@skyview.my"
            autoCapitalize="none"
            autoComplete="email"
            keyboardType="email-address"
            value={email}
            onChangeText={setEmail}
            error={emailError ?? formError}
          />

          {successMessage && <Text style={styles.success}>{successMessage}</Text>}

          <View style={styles.actions}>
            <PrimaryButton label="Send Reset Link" onPress={handleSend} loading={isSubmitting} />
            {successMessage && (
              <Pressable
                style={styles.enterCodeWrap}
                onPress={() => router.push('/(auth)/reset-password')}
              >
                <Text style={styles.enterCodeText}>Enter Reset Code →</Text>
              </Pressable>
            )}
          </View>

          <View style={styles.divider} />

          <PropertyContactCard />
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
    success: {
      fontFamily: fonts.bodyMedium,
      fontSize: 13,
      color: colors.success,
      marginTop: -6,
      marginBottom: 14,
      lineHeight: 18,
    },
    actions: {
      gap: 14,
      marginBottom: 26,
    },
    enterCodeWrap: {
      alignSelf: 'center',
    },
    enterCodeText: {
      fontFamily: fonts.bodyMedium,
      fontSize: 13,
      color: colors.accent,
    },
    divider: {
      height: 1,
      backgroundColor: colors.border,
      marginBottom: 22,
    },
  });
