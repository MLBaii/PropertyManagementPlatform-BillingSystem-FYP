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

// ISO date string -> "2h ago" / "1d ago", matching Figure 4.14's notification timestamps.
// Falls back to the short date once it's further back than a "days ago" reading stays useful.
export function formatRelativeTime(isoDate: string): string {
  const diffMs = Date.now() - new Date(isoDate).getTime();
  const diffMinutes = Math.floor(diffMs / 60000);

  if (diffMinutes < 1) {
    return 'Just now';
  }
  if (diffMinutes < 60) {
    return `${diffMinutes}m ago`;
  }
  const diffHours = Math.floor(diffMinutes / 60);
  if (diffHours < 24) {
    return `${diffHours}h ago`;
  }
  const diffDays = Math.floor(diffHours / 24);
  if (diffDays < 30) {
    return `${diffDays}d ago`;
  }
  return formatShortDate(isoDate);
}
