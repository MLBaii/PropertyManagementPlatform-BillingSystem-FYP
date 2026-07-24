export interface AdminSession { token: string; username: string; role: string; }
export interface DashboardSummary { propertyName: string; activeUnits: number; outstandingBalance: number; confirmedPayments: number; pendingPaymentProofs: number; openDisputes: number; }
export interface AdminUnit { unitId: number; unitNumber: string; floor: number; type: string; isActive: boolean; residentCount: number; }
export interface AdminBill { billId: number; referenceNumber: string; unitNumber: string; billingPeriod: string; dueDate: string; totalAmount: number; outstandingBalance: number; status: string; }
export interface AgingSummary { current: number; days1To30: number; days31To60: number; days61To90: number; over90Days: number; totalOutstanding: number; }
