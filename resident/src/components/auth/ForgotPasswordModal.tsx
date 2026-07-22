import { Feather } from '@expo/vector-icons';
import React, { useEffect, useState } from 'react';
import { ActivityIndicator, StyleSheet, Text, View } from 'react-native';

import { ModalSheet } from '@/components/ui/ModalSheet';
import { getPropertyContact, PropertyContact } from '@/services/property/propertyService';
import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = {
  visible: boolean;
  onClose: () => void;
};

export function ForgotPasswordModal({ visible, onClose }: Props) {
  const [contact, setContact] = useState<PropertyContact | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | undefined>();

  useEffect(() => {
    if (!visible) {
      return;
    }
    setIsLoading(true);
    setErrorMessage(undefined);
    getPropertyContact()
      .then(setContact)
      .catch(() => setErrorMessage('Could not load contact details. Please try again.'))
      .finally(() => setIsLoading(false));
  }, [visible]);

  return (
    <ModalSheet visible={visible} title="Forgot Password?" onClose={onClose}>
      <Text style={styles.body}>
        Password resets are handled by your property manager. Please contact them to reset
        your account.
      </Text>

      {isLoading ? (
        <ActivityIndicator color={colors.accent} style={styles.loading} />
      ) : errorMessage ? (
        <Text style={styles.error}>{errorMessage}</Text>
      ) : contact ? (
        <View style={styles.contactCard}>
          <Text style={styles.propertyName}>{contact.propertyName}</Text>
          <View style={styles.contactRow}>
            <Feather name="mail" size={14} color={colors.accent} />
            <Text style={styles.contactValue}>{contact.contactEmail}</Text>
          </View>
          <View style={styles.contactRow}>
            <Feather name="phone" size={14} color={colors.accent} />
            <Text style={styles.contactValue}>{contact.contactPhone}</Text>
          </View>
        </View>
      ) : null}
    </ModalSheet>
  );
}

const styles = StyleSheet.create({
  body: {
    fontFamily: fonts.body,
    fontSize: 14,
    lineHeight: 20,
    color: colors.text,
    marginBottom: 16,
  },
  loading: {
    marginTop: 4,
  },
  error: {
    fontFamily: fonts.bodyMedium,
    fontSize: 13,
    color: colors.danger,
  },
  contactCard: {
    backgroundColor: colors.surface2,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: 14,
    gap: 8,
  },
  propertyName: {
    fontFamily: fonts.bodySemiBold,
    fontSize: 13,
    color: colors.text,
    marginBottom: 2,
  },
  contactRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  contactValue: {
    fontFamily: fonts.body,
    fontSize: 13,
    color: colors.textSecondary,
  },
});
