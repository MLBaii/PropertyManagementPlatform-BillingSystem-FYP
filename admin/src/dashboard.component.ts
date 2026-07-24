import { DecimalPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Input, OnChanges } from '@angular/core';
import { forkJoin } from 'rxjs';

interface Summary { propertyName: string; activeUnits: number; outstandingBalance: number; confirmedPayments: number; pendingPaymentProofs: number; openDisputes: number; }
interface Bill { billId: number; referenceNumber: string; unitNumber: string; billingPeriod: string; dueDate: string; totalAmount: number; outstandingBalance: number; status: string; }
interface Aging { current: number; days1To30: number; days31To60: number; days61To90: number; over90Days: number; totalOutstanding: number; }
interface Audit { actionType: string; affectedEntity: string; description?: string; timestamp: string; }

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DecimalPipe, DatePipe],
  template: `
    @if (summary) {
      <section class="metrics">
        <article class="metric billed"><span>Total billed</span><strong>RM {{ totalBilled | number:'1.2-2' }}</strong><small>{{ summary.activeUnits }} active units</small></article>
        <article class="metric collected"><span>Total collected</span><strong>RM {{ summary.confirmedPayments | number:'1.2-2' }}</strong><small>Confirmed payments</small></article>
        <article class="metric outstanding"><span>Outstanding</span><strong>RM {{ summary.outstandingBalance | number:'1.2-2' }}</strong><small>{{ pendingCount }} pending review{{ pendingCount === 1 ? '' : 's' }}</small></article>
        <article class="metric overdue"><span>90+ days overdue</span><strong>RM {{ aging?.over90Days ?? 0 | number:'1.2-2' }}</strong><small>Requires attention</small></article>
      </section>

      <section class="dashboard-grid">
        <article class="dashboard-card overview-card">
          <div class="card-head"><div><h2>Collection overview</h2><p>Current financial position</p></div><span class="tag">Live data</span></div>
          <div class="overview-values"><div><span>Collected</span><strong>RM {{ summary.confirmedPayments | number:'1.2-2' }}</strong></div><div><span>Outstanding</span><strong class="warning">RM {{ summary.outstandingBalance | number:'1.2-2' }}</strong></div></div>
          <div class="bar-group"><div class="bar-label"><span>Collection progress</span><b>{{ collectionRate | number:'1.0-0' }}%</b></div><div class="track"><i class="collection" [style.width.%]="collectionRate"></i></div></div>
          <p class="subnote">Based on total billed records and confirmed payments.</p>
        </article>

        <article class="dashboard-card overdue-card"><div class="card-head"><div><h2>Top outstanding units</h2><p>Highest unpaid balances</p></div><span class="blue">Bills</span></div>
          @if (overdueBills.length) { @for (bill of overdueBills; track bill.billId) { <div class="unit-row"><div><b>{{ bill.unitNumber }}</b><span>{{ bill.referenceNumber }}</span></div><div class="unit-value"><strong>RM {{ bill.outstandingBalance | number:'1.2-2' }}</strong><small [class.late]="bill.status === 'Overdue'">{{ bill.status }}</small></div></div> } } @else { <p class="empty">No outstanding bills.</p> }
        </article>

        <article class="dashboard-card activity-card"><div class="card-head"><div><h2>Recent activity</h2><p>Latest admin actions</p></div></div>
          @if (activities.length) { @for (activity of activities; track activity.timestamp + activity.description) { <div class="activity"><i></i><div><b>{{ activity.description || (activity.actionType + ' ' + activity.affectedEntity) }}</b><span>{{ activity.timestamp | date:'medium' }}</span></div></div> } } @else { <p class="empty">No admin actions recorded yet.</p> }
        </article>

        <article class="dashboard-card aging-card"><div class="card-head"><div><h2>Aging summary</h2><p>Outstanding balance by age</p></div><span class="blue">Report</span></div>
          @for (bucket of agingBuckets; track bucket.label) { <div class="age-row"><span>{{ bucket.label }}</span><div class="track"><i [class]="bucket.color" [style.width.%]="bucket.percent"></i></div><b [class.urgent]="bucket.color === 'red'">RM {{ bucket.amount | number:'1.2-2' }}</b></div> }
        </article>
      </section>
    } @else { <div class="loading">Loading dashboard...</div> }
  `,
  styles: [`
    :host{display:block}.metrics{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:18px;margin:26px 0}.metric,.dashboard-card{background:#171e2d;border:1px solid #2a3854;border-radius:15px;box-shadow:0 12px 30px #070a1026}.metric{padding:20px;border-left:4px solid #3d82f6}.metric.collected{border-left-color:#24c891}.metric.outstanding{border-left-color:#f7ad28}.metric.overdue{border-left-color:#fa6262}.metric span,.metric small{display:block;color:#8fa0ba;font-size:.82rem}.metric strong{display:block;color:#edf3ff;font-size:1.6rem;margin:14px 0 6px;letter-spacing:-.03em}.dashboard-grid{display:grid;grid-template-columns:1.15fr .85fr;gap:18px}.dashboard-card{padding:22px;min-height:225px}.card-head{display:flex;justify-content:space-between;gap:16px;align-items:flex-start;margin-bottom:19px}.card-head h2{margin:0;color:#eef4ff;font-size:1.05rem}.card-head p{margin:6px 0 0;color:#8293ae;font-size:.82rem}.tag,.blue{font-size:.72rem;padding:5px 9px;border-radius:999px;background:#183b72;color:#6da7ff}.tag{background:#133d32;color:#46dfaa}.overview-values{display:grid;grid-template-columns:1fr 1fr;gap:16px}.overview-values span{display:block;color:#91a0b8;font-size:.82rem}.overview-values strong{display:block;color:#eaf1ff;font-size:1.22rem;margin-top:7px}.overview-values .warning{color:#ffc357}.bar-group{margin-top:30px}.bar-label{display:flex;justify-content:space-between;color:#aab6c9;font-size:.8rem;margin-bottom:8px}.bar-label b{color:#42d9a3}.track{height:8px;background:#101624;border-radius:10px;overflow:hidden}.track i{display:block;height:100%;border-radius:10px}.collection{background:#24c891}.subnote{color:#72839e;font-size:.76rem;margin:13px 0 0}.unit-row{display:flex;justify-content:space-between;align-items:center;border:1px solid #26324a;background:#111724;border-radius:10px;padding:11px 12px;margin-top:9px}.unit-row b{display:block;color:#e9effb;font-size:.9rem}.unit-row span{display:block;color:#7f91ab;font-size:.72rem;margin-top:3px}.unit-value{text-align:right}.unit-value strong{display:block;color:#ff8282;font-size:.86rem}.unit-value small{color:#7f91ab;font-size:.7rem}.unit-value small.late{color:#ffb14c}.activity{display:flex;gap:10px;padding:11px 0;border-bottom:1px solid #253149}.activity:last-child{border-bottom:0}.activity i{width:8px;height:8px;border-radius:50%;background:#3982ff;margin-top:5px;flex:none}.activity b{display:block;color:#dbe5f7;font-size:.82rem;font-weight:600}.activity span{color:#73839c;font-size:.72rem;display:block;margin-top:5px}.age-row{display:grid;grid-template-columns:82px 1fr 106px;align-items:center;gap:11px;margin:17px 0;color:#91a0b8;font-size:.8rem}.age-row b{text-align:right;color:#dce6f7;font-size:.8rem}.age-row .urgent{color:#ff7777}.blue{background:#183b72}.yellow{background:#f3b328}.orange{background:#fa7c29}.red{background:#f05252}.green{background:#24c891}.empty,.loading{color:#8392a9;font-size:.86rem}.loading{padding:38px 0}@media(max-width:1000px){.metrics{grid-template-columns:repeat(2,1fr)}.dashboard-grid{grid-template-columns:1fr}}@media(max-width:560px){.metrics{grid-template-columns:1fr}.overview-values{grid-template-columns:1fr}.age-row{grid-template-columns:75px 1fr 88px}.dashboard-card{padding:17px}}
  `]
})
export class DashboardComponent implements OnChanges {
  @Input({ required: true }) token = '';
  summary: Summary | null = null; aging: Aging | null = null; bills: Bill[] = []; activities: Audit[] = [];
  constructor(private http: HttpClient) {}
  ngOnChanges() { if (this.token) this.load(); }
  private headers() { return new HttpHeaders({ Authorization: `Bearer ${this.token}` }); }
  load() { forkJoin({ summary: this.http.get<Summary>('http://localhost:5112/api/admin/dashboard', { headers: this.headers() }), aging: this.http.get<Aging>('http://localhost:5112/api/admin/reports/aging-summary', { headers: this.headers() }), bills: this.http.get<Bill[]>('http://localhost:5112/api/admin/bills', { headers: this.headers() }), activities: this.http.get<Audit[]>('http://localhost:5112/api/admin/audit-log', { headers: this.headers() }) }).subscribe({ next: data => { this.summary = data.summary; this.aging = data.aging; this.bills = data.bills; this.activities = data.activities.slice(0, 4); }, error: () => { this.summary = { propertyName: 'PropertyBill', activeUnits: 0, outstandingBalance: 0, confirmedPayments: 0, pendingPaymentProofs: 0, openDisputes: 0 }; } }); }
  get totalBilled() { return this.bills.reduce((sum, bill) => sum + bill.totalAmount, 0); }
  get pendingCount() { return (this.summary?.pendingPaymentProofs ?? 0) + (this.summary?.openDisputes ?? 0); }
  get collectionRate() { const total = this.totalBilled || (this.summary?.confirmedPayments ?? 0) + (this.summary?.outstandingBalance ?? 0); return total ? Math.min(100, ((this.summary?.confirmedPayments ?? 0) / total) * 100) : 0; }
  get overdueBills() { return this.bills.filter(bill => bill.outstandingBalance > 0).sort((a, b) => b.outstandingBalance - a.outstandingBalance).slice(0, 4); }
  get agingBuckets() { const a = this.aging; const total = a?.totalOutstanding || 1; return [{ label: 'Current', amount: a?.current ?? 0, color: 'green' }, { label: '1-30 days', amount: a?.days1To30 ?? 0, color: 'blue' }, { label: '31-60 days', amount: a?.days31To60 ?? 0, color: 'yellow' }, { label: '61-90 days', amount: a?.days61To90 ?? 0, color: 'orange' }, { label: '90+ days', amount: a?.over90Days ?? 0, color: 'red' }].map(bucket => ({ ...bucket, percent: Math.min(100, (bucket.amount / total) * 100) })); }
}
