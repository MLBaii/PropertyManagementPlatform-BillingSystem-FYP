import { apiClient } from '@/services/api/client';

export type ActivityType = 'BillIssued' | 'PaymentConfirmed';

export type ActivityItem = {
  type: ActivityType;
  date: string;
  description: string;
  reference: string | null;
  amount: number;
};

export type Dashboard = {
  unitNumber: string;
  propertyName: string;
  totalOutstanding: number;
  totalPaid: number;
  creditBalance: number;
  recentActivity: ActivityItem[];
};

export async function getDashboard(): Promise<Dashboard> {
  const { data } = await apiClient.get<Dashboard>('/residents/dashboard');
  return data;
}
