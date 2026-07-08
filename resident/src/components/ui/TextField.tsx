import React from 'react';
import { StyleSheet, Text, TextInput, TextInputProps, View } from 'react-native';

import { colors } from '@/theme/colors';
import { fonts } from '@/theme/typography';

type Props = TextInputProps & {
  label: string;
  error?: string;
};

export function TextField({ label, error, style, ...inputProps }: Props) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <TextInput
        placeholderTextColor={colors.textSecondary}
        style={[styles.input, error ? styles.inputError : null, style]}
        {...inputProps}
      />
      {error ? <Text style={styles.errorText}>{error}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  field: {
    marginBottom: 14,
  },
  label: {
    fontSize: 12,
    color: colors.textSecondary,
    marginBottom: 7,
    fontFamily: fonts.bodyMedium,
  },
  input: {
    height: 48,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    paddingHorizontal: 14,
    color: colors.text,
    fontSize: 14,
    fontFamily: fonts.body,
  },
  inputError: {
    borderColor: colors.danger,
  },
  errorText: {
    color: colors.danger,
    fontSize: 11,
    marginTop: 6,
    fontFamily: fonts.body,
  },
});
