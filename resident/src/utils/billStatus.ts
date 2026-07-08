import { colors } from '@/theme/colors';
import { Bill } from '@/services/bills/billsService';

export function getCountdownColor(status: Bill['status']): string {
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
