// Warm dark editorial palette — see mockups/propertybill-mockups.html for the reference design.
export const colors = {
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
  surface2: '#2B2520',
} as const;

export type ThemeColors = typeof colors;
