// Warm editorial palette — see mockups/propertybill-mockups.html for the reference design.
// Dark is the original/default look; light keeps the same terracotta accent and warm
// undertone but swaps the surfaces and darkens/saturates the status colors so they stay
// readable against a white background instead of a dark one.
export const darkColors = {
  background: '#1A1614',
  surface: '#221C18',
  border: '#3A3530',
  text: '#F5F1EA',
  textSecondary: '#A89B8E',
  accent: '#D67D5C',
  accentSoft: 'rgba(214, 125, 92, 0.12)',
  accentLine: 'rgba(214, 125, 92, 0.3)',
  onAccent: '#1A1410',
  danger: '#D97560',
  dangerBg: 'rgba(217, 117, 96, 0.14)',
  success: '#7CB596',
  successBg: 'rgba(124, 181, 150, 0.12)',
  unpaid: '#6FA8D4',
  unpaidBg: 'rgba(111, 168, 212, 0.12)',
  pending: '#D9B260',
  pendingBg: 'rgba(217, 178, 96, 0.12)',
  disputed: '#B08FD9',
  disputedBg: 'rgba(176, 143, 217, 0.12)',
  pendingDispute: '#9C8FB0',
  pendingDisputeBg: 'rgba(156, 143, 176, 0.12)',
  surface2: '#2B2520',
} as const;

export const lightColors = {
  background: '#FAF7F2',
  surface: '#FFFFFF',
  border: '#DED7CB',
  text: '#221C18',
  textSecondary: '#6B6258',
  accent: '#D67D5C',
  accentSoft: 'rgba(214, 125, 92, 0.10)',
  accentLine: 'rgba(214, 125, 92, 0.35)',
  onAccent: '#1A1410',
  danger: '#C2503A',
  dangerBg: 'rgba(194, 80, 58, 0.10)',
  success: '#3F8A67',
  successBg: 'rgba(63, 138, 103, 0.10)',
  unpaid: '#3E76A8',
  unpaidBg: 'rgba(62, 118, 168, 0.10)',
  pending: '#B08228',
  pendingBg: 'rgba(176, 130, 40, 0.10)',
  disputed: '#8862B0',
  disputedBg: 'rgba(136, 98, 176, 0.10)',
  pendingDispute: '#756A87',
  pendingDisputeBg: 'rgba(117, 106, 135, 0.10)',
  surface2: '#F1ECE3',
} as const;

// Widened to plain strings (not the literal hex union `as const` would otherwise infer from
// darkColors alone) so lightColors — a different set of literal values for the same keys —
// structurally satisfies this type too.
export type ThemeColors = Record<keyof typeof darkColors, string>;
export type ThemeName = 'dark' | 'light';

export const palettes: Record<ThemeName, ThemeColors> = {
  dark: darkColors,
  light: lightColors,
};

// Kept as a fallback default (= dark, the app's original look) for any call site that isn't
// theme-aware — everything that renders visible chrome should prefer useTheme().colors instead.
export const colors = darkColors;
