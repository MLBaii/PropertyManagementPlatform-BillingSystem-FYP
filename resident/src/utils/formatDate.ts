const MONTH_NAMES = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

// "2026-04" -> "April 2026"
export function formatBillingPeriod(billingPeriod: string): string {
  const [year, month] = billingPeriod.split('-').map(Number);
  const monthName = MONTH_NAMES[(month ?? 1) - 1] ?? billingPeriod;
  return `${monthName} ${year}`;
}

// ISO date string -> "15 Oct 2026"
export function formatShortDate(isoDate: string): string {
  const date = new Date(isoDate);
  const day = date.getUTCDate();
  const month = MONTH_NAMES[date.getUTCMonth()].slice(0, 3);
  return `${day} ${month} ${date.getUTCFullYear()}`;
}
