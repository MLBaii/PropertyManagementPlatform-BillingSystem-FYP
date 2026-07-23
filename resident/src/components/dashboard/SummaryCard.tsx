import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { Card } from '@/components/ui/Card';
import { ThemeColors } from '@/theme/colors';
import { useTheme } from '@/theme/ThemeContext';
import { fonts } from '@/theme/typography';

type Badge = {
  label: string;
  color: string;
  bg: string;
};

type Props = {
  eyebrow: string;
  amount: number;
  color: string;
  variant?: 'primary' | 'secondary';
  badge?: Badge;
};

export function SummaryCard({ eyebrow, amount, color, variant = 'secondary', badge }: Props) {
  const { colors } = useTheme();
  const styles = createStyles(colors);
  const isPrimary = variant === 'primary';

  return (
    <Card style={[styles.card, isPrimary && { borderColor: color }]}>
      <View style={styles.topRow}>
        <Text style={styles.eyebrow}>{eyebrow}</Text>
        {badge && (
          <View style={[styles.badge, { backgroundColor: badge.bg }]}>
            <View style={[styles.dot, { backgroundColor: badge.color }]} />
            <Text style={[styles.badgeLabel, { color: badge.color }]}>{badge.label}</Text>
          </View>
        )}
      </View>
      <Text style={[isPrimary ? styles.amountPrimary : styles.amountSecondary, { color }]}>
        RM {amount.toFixed(2)}
      </Text>
    </Card>
  );
}

const createStyles = (colors: ThemeColors) =>
  StyleSheet.create({
    card: {
      flex: 1,
    },
    topRow: {
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
    },
    eyebrow: {
      fontFamily: fonts.body,
      fontSize: 10,
      letterSpacing: 1.5,
      textTransform: 'uppercase',
      color: colors.textSecondary,
    },
    badge: {
      flexDirection: 'row',
      alignItems: 'center',
      gap: 5,
      paddingHorizontal: 10,
      paddingVertical: 4,
      borderRadius: 20,
    },
    dot: {
      width: 6,
      height: 6,
      borderRadius: 3,
    },
    badgeLabel: {
      fontFamily: fonts.bodySemiBold,
      fontSize: 11,
    },
    amountPrimary: {
      fontFamily: fonts.heading,
      fontSize: 32,
      marginTop: 8,
    },
    amountSecondary: {
      fontFamily: fonts.heading,
      fontSize: 20,
      marginTop: 6,
    },
  });
