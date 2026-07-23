import { Bill } from '@/services/bills/billsService';
import { ThemeColors } from '@/theme/colors';

// Purely payment-side, like Bill['status'] itself — dispute state is a separate badge
// (see BillCard/[billId].tsx rendering ActiveDisputeStatus alongside this), never folded
// into the countdown row. Takes colors as a parameter (not a hook) since this is a plain
// function, not a component — callers get colors from useTheme() themselves.
export function getCountdownColor(status: Bill['status'], colors: ThemeColors): string {
  switch (status) {
    case 'Paid':
      return colors.success;
    case 'Overdue':
      return colors.danger;
    case 'ProofSubmitted':
      return colors.pending;
    default:
      return colors.unpaid;
  }
}

// Primary countdown line: "Settled" / "Pending review" / "N days overdue" / "N days left" / "Due today".
export function getCountdownLabel(bill: Pick<Bill, 'status' | 'daysUntilDue'>): string {
  if (bill.status === 'Paid') {
    return 'Settled';
  }
  if (bill.status === 'ProofSubmitted') {
    return 'Pending review';
  }
  if (bill.status === 'Overdue') {
    return `${Math.abs(bill.daysUntilDue)} days overdue`;
  }
  if (bill.daysUntilDue === 0) {
    return 'Due today';
  }
  return `${bill.daysUntilDue} days left`;
}

// Whether to show the secondary "Due {date}" line — omitted once settled or under review.
export function shouldShowDueDateLine(status: Bill['status']): boolean {
  return status === 'Unpaid' || status === 'Overdue';
}
