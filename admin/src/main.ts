import { bootstrapApplication } from '@angular/platform-browser';
import { Component } from '@angular/core';
import { HttpClient, provideHttpClient } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuditLogComponent } from './audit-log.component';
import { BillingConfigurationComponent } from './billing-configuration.component';
import { BillsComponent } from './bills.component';
import { DashboardComponent } from './dashboard.component';
import { PaymentsComponent } from './payments.component';
import { PropertiesComponent } from './properties.component';
import { RemindersComponent } from './reminders.component';
import { ReportsComponent } from './reports.component';

interface Login { token: string; username: string; role: string; }
interface Dash { propertyName: string; activeUnits: number; outstandingBalance: number; confirmedPayments: number; pendingPaymentProofs: number; openDisputes: number; }
interface Unit { unitId: number; unitNumber: string; floor: number; type: string; isActive: boolean; residentCount: number; }
interface Item { billingItemId: number; chargeType: string; defaultRate: number; frequency: string; billingDay: number; dueDay: number; penaltyRate: number; gracePeriodDays: number; }
interface Bill { billId: number; referenceNumber: string; unitNumber: string; billingPeriod: string; dueDate: string; totalAmount: number; outstandingBalance: number; status: string; }
type Page = 'Dashboard' | 'Properties & Units' | 'Billing configuration' | 'Bills' | 'Payments' | 'Reports' | 'Reminders' | 'Audit log';

@Component({
  selector: 'app-root', standalone: true,
  imports: [ReactiveFormsModule, DashboardComponent, ReportsComponent, RemindersComponent, AuditLogComponent, PaymentsComponent, PropertiesComponent, BillingConfigurationComponent, BillsComponent],
  template: `
    @if (!s) {
      <main class="login"><section><b>Property<span>Bill</span></b><small>ADMIN PORTAL</small><h1>Welcome back</h1><p>Sign in to manage property billing.</p><form [formGroup]="lf" (ngSubmit)="login()"><label>Username<input placeholder="Username" formControlName="username"></label><label>Password<input type="password" placeholder="Password" formControlName="password"></label>@if(err){<p class="error">{{err}}</p>}<button>Log in</button></form></section></main>
    } @else {
      <div class="shell">
        <aside><div class="brand"><b>Property<span>Bill</span></b><small>Admin Portal</small></div><nav><small>MAIN</small><a (click)="select('Dashboard')" [class.active]="p==='Dashboard'">Dashboard</a><a (click)="select('Properties & Units')" [class.active]="p==='Properties & Units'">Properties & units</a><small>BILLING</small><a (click)="select('Billing configuration')" [class.active]="p==='Billing configuration'">Billing configuration</a><a (click)="select('Bills')" [class.active]="p==='Bills'">Bills</a><a (click)="select('Payments')" [class.active]="p==='Payments'">Payments</a><a (click)="select('Reports')" [class.active]="p==='Reports'">Reports</a><small>SYSTEM</small><a (click)="select('Reminders')" [class.active]="p==='Reminders'">Reminders</a><a (click)="select('Audit log')" [class.active]="p==='Audit log'">Audit log</a></nav><button class="logout" (click)="logout()">Log out</button></aside>
        <main class="content"><header><div><h1>{{p}}</h1><p>{{d?.propertyName || 'PropertyBill'}} — billing management</p></div><div class="profile"><span>{{s.username.slice(0,2).toUpperCase()}}</span><b>{{s.username}}</b></div></header>
          @if(p==='Dashboard'){<app-dashboard [token]="s.token"/>}
          @else if(p==='Properties & Units'){<app-properties [token]="s.token" (changed)="ld()"/>}
          @else if(p==='Billing configuration'){<app-billing-configuration [token]="s.token"/>}
          @else if(p==='Bills'){<app-bills [token]="s.token"/>}
          @else if(p==='Payments'){<app-payments [token]="s.token"/>}
          @else if(p==='Reports'){<app-reports [token]="s.token"/>}
          @else if(p==='Reminders'){<app-reminders [token]="s.token"/>}
          @else if(p==='Audit log'){<app-audit-log [token]="s.token"/>}
        </main>
      </div>
    }
  `,
  styles: [`
    :host{font-family:Inter,Arial,sans-serif;color:#e9effb}.login{min-height:100vh;display:grid;place-items:center;background:radial-gradient(circle at 20% 10%,#1e3157,#0d111b 55%)}.login section{width:min(390px,calc(100% - 48px));padding:34px;background:#171e2d;border:1px solid #2a3854;border-radius:16px;box-shadow:0 20px 60px #0007}.login b,.brand b{display:block;font-size:1.35rem;color:#eff4ff}.login b span,.brand b span{color:#4a91ff}.login small{display:block;color:#7f91ab;letter-spacing:.12em;margin-top:22px}.login h1{margin:8px 0;color:#eef4ff}.login p{color:#93a2b9}.login form{display:grid;gap:14px;margin-top:24px}.login label,.row label,.item-form label{display:grid;gap:7px;color:#91a0b8;font-size:.8rem;font-weight:700}input,select{box-sizing:border-box;width:100%;padding:11px 12px;border:1px solid #34435e;background:#101624;border-radius:8px;color:#eef4ff;font:inherit}button{border:0;border-radius:8px;padding:11px 15px;background:#347cf2;color:#fff;font-weight:700;cursor:pointer}.shell{min-height:100vh;display:grid;grid-template-columns:250px 1fr;background:#0d111b}.shell aside{position:sticky;top:0;height:100vh;display:flex;flex-direction:column;background:#151b29;border-right:1px solid #28344a}.brand{padding:28px 28px 24px;border-bottom:1px solid #28344a}.brand small{display:block;color:#7587a1;margin-top:4px}.shell nav{display:flex;flex-direction:column;gap:4px;padding:22px 14px}.shell nav small{color:#6e7f9a;font-size:.72rem;letter-spacing:.1em;margin:14px 14px 4px}.shell nav small:first-child{margin-top:0}.shell nav a{padding:11px 14px;border-radius:8px;color:#9baac1;cursor:pointer;font-size:.92rem}.shell nav a:hover{background:#1c263a;color:#edf3ff}.shell nav a.active{background:#1c3156;color:#62a0ff;box-shadow:inset 3px 0 #3d82f6}.logout{margin:auto 18px 20px;background:transparent;border:1px solid #3a4b67;color:#a8b6cc}.content{padding:0 38px 42px;min-width:0}.content header{display:flex;justify-content:space-between;align-items:center;padding:24px 0;border-bottom:1px solid #273348}.content header h1{margin:0;color:#f1f5ff;font-size:1.7rem}.content header p{margin:6px 0 0;color:#8293ae}.profile{display:flex;align-items:center;gap:10px;color:#b3c1d5;font-size:.86rem}.profile span{display:grid;place-items:center;width:36px;height:36px;border:2px solid #3e86ff;border-radius:50%;color:#65a0ff}.panel{margin-top:28px;padding:22px;background:#171e2d;border:1px solid #2a3854;border-radius:15px}.head{display:flex;justify-content:space-between;gap:16px;align-items:center}.head h2{margin:0;color:#eef4ff}.head p{margin:6px 0 0;color:#8293ae;font-size:.84rem}.row{display:grid;grid-template-columns:1fr 120px 1fr auto;gap:12px;margin-top:20px;align-items:end}.item-form{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin-top:20px;padding:16px;background:#121927;border:1px solid #2a3854;border-radius:10px;align-items:end}.table-wrap{overflow:auto}table{width:100%;min-width:720px;border-collapse:collapse;margin-top:18px}th,td{text-align:left;padding:13px 12px;border-top:1px solid #2a364c;color:#cbd6e7}th{color:#8293ae;font-size:.76rem;text-transform:uppercase;letter-spacing:.04em}.error{color:#ff8b8b}.status{display:inline-block;padding:4px 8px;border-radius:10px;background:#103b30;color:#47dca8;font-size:.78rem}.status.overdue{background:#51262b;color:#ff9898}@media(max-width:900px){.shell{grid-template-columns:1fr}.shell aside{display:none}.content{padding:0 18px 30px}.row,.item-form{grid-template-columns:1fr 1fr}}@media(max-width:560px){.content header{align-items:flex-start}.profile b{display:none}.row,.item-form{grid-template-columns:1fr}}
  `]
})
class App {
  p: Page = 'Dashboard'; s: Login | null = null; d: Dash | null = null; us: Unit[] = []; is: Item[] = []; bs: Bill[] = []; su = false; si = false; err = ''; msg = '';
  lf = new FormGroup({ username: new FormControl('', { nonNullable: true, validators: [Validators.required] }), password: new FormControl('', { nonNullable: true, validators: [Validators.required] }) });
  uf = new FormGroup({ unitNumber: new FormControl('', { nonNullable: true, validators: [Validators.required] }), floor: new FormControl(1, { nonNullable: true }), type: new FormControl('', { nonNullable: true, validators: [Validators.required] }) });
  itemForm = new FormGroup({ chargeType: new FormControl('', { nonNullable: true, validators: [Validators.required] }), defaultRate: new FormControl(0, { nonNullable: true }), frequency: new FormControl('Monthly', { nonNullable: true }), billingDay: new FormControl(1, { nonNullable: true }), dueDay: new FormControl(15, { nonNullable: true }), penaltyRate: new FormControl(0, { nonNullable: true }), gracePeriodDays: new FormControl(0, { nonNullable: true }) });
  constructor(private http: HttpClient) {}
  h() { return { Authorization: `Bearer ${this.s?.token ?? ''}` }; }
  login() { this.http.post<Login>('http://localhost:5112/api/auth/admin/login', this.lf.getRawValue()).subscribe({ next: data => { this.s = data; this.ld(); }, error: () => this.err = 'Invalid username or password.' }); }
  select(page: Page) { this.p = page; if (page === 'Properties & Units') this.lu(); if (page === 'Billing configuration') this.li(); if (page === 'Bills') this.loadB(); }
  ld() { this.http.get<Dash>('http://localhost:5112/api/admin/dashboard', { headers: this.h() }).subscribe(data => this.d = data); }
  lu() { this.http.get<Unit[]>('http://localhost:5112/api/admin/properties/units', { headers: this.h() }).subscribe(data => this.us = data); }
  li() { this.http.get<Item[]>('http://localhost:5112/api/admin/billing-items', { headers: this.h() }).subscribe(data => this.is = data); }
  loadB() { this.http.get<Bill[]>('http://localhost:5112/api/admin/bills', { headers: this.h() }).subscribe(data => this.bs = data); }
  addU() { this.http.post('http://localhost:5112/api/admin/properties/units', this.uf.getRawValue(), { headers: this.h() }).subscribe({ next: () => { this.su = false; this.lu(); this.ld(); }, error: error => this.msg = error.status === 409 ? 'This unit number already exists.' : 'Unable to save unit.' }); }
  addI() { this.http.post('http://localhost:5112/api/admin/billing-items', this.itemForm.getRawValue(), { headers: this.h() }).subscribe({ next: () => { this.si = false; this.li(); }, error: error => this.msg = error.status === 409 ? 'This charge type already exists.' : 'Unable to save item.' }); }
  logout() { this.s = null; this.d = null; this.us = []; this.is = []; this.bs = []; }
}
bootstrapApplication(App, { providers: [provideHttpClient()] });
