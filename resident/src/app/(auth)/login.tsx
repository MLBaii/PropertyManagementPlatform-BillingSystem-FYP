import axios from 'axios';
import { router, useLocalSearchParams } from 'expo-router';
import React, { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { ForgotPasswordModal } from '@/components/auth/ForgotPasswordModal';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { Screen } from '@/components/ui/Screen';
import { TextField } from '@/components/ui/TextField';
import { useAuth } from '@/services/auth/AuthContext';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

function isValidEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

export default function LoginScreen() {
  const { login } = useAuth();
  const { sessionExpired } = useLocalSearchParams<{ sessionExpired?: string }>();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [emailError, setEmailError] = useState<string | undefined>();
  const [passwordError, setPasswordError] = useState<string | undefined>();
  // UC-101 A6: pre-filled when the axios interceptor redirects here after a 401.
  const [formError, setFormError] = useState<string | undefined>(
    sessionExpired === '1' ? 'Your session has expired. Please log in again.' : undefined
  );
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isForgotPasswordVisible, setIsForgotPasswordVisible] = useState(false);

  const handleLogin = async () => {
    setFormError(undefined);

    let hasError = false;
    if (!email.trim()) {
      setEmailError('Email is required.');
      hasError = true;
    } else if (!isValidEmail(email.trim())) {
      setEmailError('Enter a valid email address.');
      hasError = true;
    } else {
      setEmailError(undefined);
    }

    if (!password) {
      setPasswordError('Password is required.');
      hasError = true;
    } else {
      setPasswordError(undefined);
    }

    if (hasError) {
      return;
    }

    setIsSubmitting(true);
    try {
      await login(email.trim(), password);
      router.replace('/(tabs)/home');
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 401) {
        setFormError('Invalid email or password. Please try again.');
      } else if (axios.isAxiosError(err) && err.response?.status === 403) {
        setFormError('This account is disabled. Contact your property manager.');
      } else {
        setFormError('Could not reach the server. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Screen>
      <KeyboardAvoidingView
        style={styles.flex}
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      >
        <ScrollView contentContainerStyle={styles.scrollContent} keyboardShouldPersistTaps="handled">
          <View style={styles.logoWrap}>
            <View style={styles.logoBadge}>
              <Text style={styles.logoLetter}>P</Text>
            </View>
            <Text style={styles.title}>Welcome back</Text>
            <Text style={styles.subtitle}>Sign in to your billing account</Text>
          </View>

          <TextField
            label="Email"
            placeholder="resident@skyview.my"
            autoCapitalize="none"
            autoComplete="email"
            keyboardType="email-address"
            value={email}
            onChangeText={setEmail}
            error={emailError}
          />
          <TextField
            label="Password"
            placeholder="••••••••"
            isPassword
            autoCapitalize="none"
            value={password}
            onChangeText={setPassword}
            error={passwordError ?? formError}
          />

          <Pressable style={styles.forgotWrap} onPress={() => setIsForgotPasswordVisible(true)}>
            <Text style={styles.forgotText}>Forgot Password?</Text>
          </Pressable>

          <PrimaryButton label="Log In" onPress={handleLogin} loading={isSubmitting} />

          <Text style={styles.footnote}>Accounts are issued by your property manager</Text>
        </ScrollView>
      </KeyboardAvoidingView>

      <ForgotPasswordModal
        visible={isForgotPasswordVisible}
        onClose={() => setIsForgotPasswordVisible(false)}
      />
    </Screen>
  );
}

const styles = StyleSheet.create({
  flex: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
    paddingVertical: 32,
  },
  logoWrap: {
    alignItems: 'center',
    marginBottom: 34,
  },
  logoBadge: {
    width: 60,
    height: 60,
    borderRadius: 16,
    backgroundColor: colors.accentSoft,
    borderWidth: 1,
    borderColor: colors.accentLine,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 18,
  },
  logoLetter: {
    fontFamily: fonts.heading,
    fontSize: 28,
    color: colors.accent,
  },
  title: {
    fontFamily: fonts.heading,
    fontSize: 26,
    letterSpacing: -0.52, // -0.02em at 26px, per .screen-title in the mockup
    color: colors.text,
    marginBottom: 6,
  },
  subtitle: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
  },
  forgotWrap: {
    alignSelf: 'flex-end',
    marginBottom: 22,
  },
  forgotText: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.accent,
  },
  footnote: {
    fontFamily: fonts.body,
    fontSize: 11,
    color: colors.textSecondary,
    textAlign: 'center',
    marginTop: 20,
  },
});
