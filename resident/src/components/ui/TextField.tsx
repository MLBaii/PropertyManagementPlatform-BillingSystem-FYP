import { Feather } from '@expo/vector-icons';
import React, { useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, TextInputProps, View } from 'react-native';

import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

type Props = TextInputProps & {
  label: string;
  error?: string;
  // Renders a show/hide eye toggle and manages the masking itself — pass this instead of
  // `secureTextEntry` directly so Login and Change Password don't each need their own
  // visibility state.
  isPassword?: boolean;
};

export function TextField({ label, error, style, editable, isPassword, ...inputProps }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const [isVisible, setIsVisible] = useState(false);

  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <View style={styles.inputWrap}>
        <TextInput
          placeholderTextColor={colors.textSecondary}
          editable={editable}
          secureTextEntry={isPassword ? !isVisible : inputProps.secureTextEntry}
          style={[
            styles.input,
            isPassword ? styles.inputWithIcon : null,
            editable === false ? styles.inputDisabled : null,
            error ? styles.inputError : null,
            style,
          ]}
          {...inputProps}
        />
        {isPassword && (
          <Pressable
            onPress={() => setIsVisible((prev) => !prev)}
            hitSlop={10}
            style={styles.eyeButton}
          >
            <Feather name={isVisible ? 'eye-off' : 'eye'} size={18} color={colors.textSecondary} />
          </Pressable>
        )}
      </View>
      {error ? <Text style={styles.errorText}>{error}</Text> : null}
    </View>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    field: {
      marginBottom: 14,
    },
    label: {
      fontSize: 12,
      color: colors.textSecondary,
      marginBottom: 7,
      fontFamily: fonts.bodyMedium,
    },
    inputWrap: {
      justifyContent: 'center',
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
    inputWithIcon: {
      paddingRight: 44,
    },
    eyeButton: {
      position: 'absolute',
      right: 14,
      height: 48,
      justifyContent: 'center',
    },
    inputError: {
      borderColor: colors.danger,
    },
    inputDisabled: {
      color: colors.textSecondary,
      opacity: 0.7,
    },
    errorText: {
      color: colors.danger,
      fontSize: 11,
      marginTop: 6,
      fontFamily: fonts.body,
    },
  });
