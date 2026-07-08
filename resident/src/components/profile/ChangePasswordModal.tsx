import axios from 'axios';
import React, { useState } from 'react';
import { StyleSheet, Text } from 'react-native';

import { ModalSheet } from '@/components/ui/ModalSheet';
import { PrimaryButton } from '@/components/ui/PrimaryButton';
import { TextField } from '@/components/ui/TextField';
import { changePassword } from '@/services/profile/profileService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  visible: boolean;
  onClose: () => void;
};

export function ChangePasswordModal({ visible, onClose }: Props) {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [currentError, setCurrentError] = useState<string | undefined>();
  const [newError, setNewError] = useState<string | undefined>();
  const [confirmError, setConfirmError] = useState<string | undefined>();
  const [successMessage, setSuccessMessage] = useState<string | undefined>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const reset = () => {
    setCurrentPassword('');
    setNewPassword('');
    setConfirmPassword('');
    setCurrentError(undefined);
    setNewError(undefined);
    setConfirmError(undefined);
    setSuccessMessage(undefined);
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  const handleSubmit = async () => {
    setSuccessMessage(undefined);
    setCurrentError(undefined);
    setNewError(undefined);
    setConfirmError(undefined);

    let hasError = false;
    if (!currentPassword) {
      setCurrentError('Current password is required.');
      hasError = true;
    }
    if (newPassword.length < 8) {
      setNewError('New password must be at least 8 characters.');
      hasError = true;
    }
    if (confirmPassword !== newPassword) {
      setConfirmError('Passwords do not match.');
      hasError = true;
    }
    if (hasError) {
      return;
    }

    setIsSubmitting(true);
    try {
      const { message } = await changePassword(currentPassword, newPassword);
      setSuccessMessage(message);
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 401) {
        setCurrentError('Current password is incorrect.');
      } else {
        setCurrentError('Could not update password. Please try again.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <ModalSheet visible={visible} title="Change Password" onClose={handleClose}>
      {successMessage ? <Text style={styles.success}>{successMessage}</Text> : null}
      <TextField
        label="Current password"
        secureTextEntry
        autoCapitalize="none"
        value={currentPassword}
        onChangeText={setCurrentPassword}
        error={currentError}
      />
      <TextField
        label="New password"
        secureTextEntry
        autoCapitalize="none"
        value={newPassword}
        onChangeText={setNewPassword}
        error={newError}
      />
      <TextField
        label="Confirm new password"
        secureTextEntry
        autoCapitalize="none"
        value={confirmPassword}
        onChangeText={setConfirmPassword}
        error={confirmError}
      />
      <PrimaryButton label="Update Password" onPress={handleSubmit} loading={isSubmitting} />
    </ModalSheet>
  );
}

const styles = StyleSheet.create({
  success: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.success,
    marginBottom: 14,
  },
});
