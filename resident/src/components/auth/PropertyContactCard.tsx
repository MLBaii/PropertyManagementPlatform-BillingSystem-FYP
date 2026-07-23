import { Feather } from '@expo/vector-icons';
import React, { useEffect, useState } from 'react';
import { ActivityIndicator, StyleSheet, Text, View } from 'react-native';

import { getPropertyContact, PropertyContact } from '@/services/property/propertyService';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

// The fallback path for a resident who can't complete the email-based reset (UC-101 A1) —
// e.g. no access to their registered inbox. Self-contained (fetches on mount) so it can be
// dropped into any pre-login screen as a secondary section.
export function PropertyContactCard() {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const [contact, setContact] = useState<PropertyContact | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | undefined>();

  useEffect(() => {
    getPropertyContact()
      .then(setContact)
      .catch(() => setErrorMessage('Could not load contact details. Please try again.'))
      .finally(() => setIsLoading(false));
  }, []);

  return (
    <View style={styles.wrap}>
      <Text style={styles.heading}>Need help? Contact your property manager</Text>
      <Text style={styles.body}>
        Password resets are also handled by your property manager if you'd rather not wait
        for the email.
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
    </View>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    wrap: {
      marginTop: 8,
    },
    heading: {
      fontFamily: fonts.bodySemiBold,
      fontSize: 13,
      color: colors.text,
      marginBottom: 6,
    },
    body: {
      fontFamily: fonts.body,
      fontSize: 12,
      lineHeight: 17,
      color: colors.textSecondary,
      marginBottom: 12,
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
