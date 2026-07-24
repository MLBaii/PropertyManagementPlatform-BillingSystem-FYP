import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Unit { unitId: number; unitNumber: string; floor: number; type: string; isActive: boolean; residentCount: number; }

@Component({
  selector: 'app-properties', standalone: true, imports: [FormsModule],
  template: `
    <section class="metrics">
      <article><span>Total units</span><strong>{{ units.length }}</strong><small>All registered units</small></article>
      <article><span>Active units</span><strong class="green">{{ activeCount }}</strong><small>Available for billing</small></article>
      <article><span>Occupied units</span><strong class="blue">{{ occupiedCount }}</strong><small>With registered residents</small></article>
      <article><span>Vacant units</span><strong class="orange">{{ vacantCount }}</strong><small>Awaiting occupancy</small></article>
    </section>

    <section class="panel">
      <div class="head"><div><h2>Properties & units</h2><p>View and maintain the residential unit register.</p></div><button (click)="showForm = !showForm">{{ showForm ? 'Cancel' : '+ Add unit' }}</button></div>
      @if (showForm) {
        <form class="unit-form" (ngSubmit)="add()"><label>Unit number<input [(ngModel)]="draft.unitNumber" name="unitNumber" placeholder="e.g. A-01-01" required></label><label>Floor<input [(ngModel)]="draft.floor" name="floor" type="number" min="0" required></label><label>Unit type<input [(ngModel)]="draft.type" name="type" placeholder="e.g. 2-Bedroom" required></label><button [disabled]="saving">{{ saving ? 'Saving...' : 'Save unit' }}</button></form>
      }
      @if (message) { <p class="message" [class.error]="isError">{{ message }}</p> }
      <div class="toolbar"><input [(ngModel)]="query" placeholder="Search unit number or type..."><span>{{ filtered.length }} unit{{ filtered.length === 1 ? '' : 's' }} shown</span></div>
      @if (loading) { <p class="empty">Loading units...</p> } @else if (!filtered.length) { <p class="empty">No units match your search.</p> } @else { <div class="table-wrap"><table><thead><tr><th>Unit</th><th>Floor</th><th>Unit type</th><th>Residents</th><th>Status</th></tr></thead><tbody>@for (unit of filtered; track unit.unitId) { <tr><td><b>{{ unit.unitNumber }}</b></td><td>Level {{ unit.floor }}</td><td>{{ unit.type }}</td><td><span class="resident-count">{{ unit.residentCount }}</span></td><td><span class="status" [class.inactive]="!unit.isActive">{{ unit.isActive ? 'Active' : 'Inactive' }}</span></td></tr> }</tbody></table></div> }
    </section>
  `,
  styles: [`
    :host{display:block}.metrics{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:18px;margin:26px 0}.metrics article,.panel{background:#171e2d;border:1px solid #2a3854;border-radius:15px;box-shadow:0 12px 30px #070a1026}.metrics article{padding:19px}.metrics span,.metrics small{display:block;color:#8fa0ba;font-size:.8rem}.metrics strong{display:block;color:#edf3ff;font-size:1.55rem;margin:13px 0 5px}.metrics .green{color:#42d9a3}.metrics .blue{color:#69a4ff}.metrics .orange{color:#ffbf55}.panel{padding:22px}.head{display:flex;justify-content:space-between;align-items:center;gap:16px}.head h2{margin:0;color:#eef4ff;font-size:1.1rem}.head p{margin:6px 0 0;color:#8293ae;font-size:.83rem}.head button,.unit-form button{border:0;border-radius:8px;padding:11px 15px;background:#347cf2;color:#fff;font-weight:700;cursor:pointer}.unit-form{display:grid;grid-template-columns:1.2fr .7fr 1.3fr auto;gap:12px;align-items:end;margin-top:20px;padding:16px;background:#121927;border:1px solid #2a3854;border-radius:10px}.unit-form label{display:grid;gap:7px;color:#91a0b8;font-size:.78rem;font-weight:700}.unit-form input,.toolbar input{box-sizing:border-box;width:100%;padding:11px 12px;border:1px solid #34435e;background:#101624;border-radius:8px;color:#eef4ff;font:inherit}.unit-form button:disabled{opacity:.6}.message{margin:15px 0;color:#47dca8;font-size:.85rem;font-weight:700}.message.error{color:#ff9292}.toolbar{display:flex;justify-content:space-between;align-items:center;gap:18px;margin-top:22px;padding:12px 0}.toolbar input{max-width:365px}.toolbar span{color:#8293ae;font-size:.8rem}.table-wrap{overflow:auto}table{width:100%;min-width:650px;border-collapse:collapse}th,td{text-align:left;padding:14px 12px;border-top:1px solid #2a364c;color:#cbd6e7}th{color:#8293ae;font-size:.76rem;text-transform:uppercase;letter-spacing:.04em}td b{color:#eef4ff}.resident-count{display:grid;place-items:center;width:26px;height:26px;background:#1d365e;color:#78adff;border-radius:50%;font-size:.78rem;font-weight:700}.status{display:inline-block;padding:4px 8px;border-radius:10px;background:#103b30;color:#47dca8;font-size:.77rem}.status.inactive{background:#41262b;color:#ff9999}.empty{padding:28px 0;color:#8495ad;text-align:center}@media(max-width:900px){.metrics{grid-template-columns:repeat(2,1fr)}.unit-form{grid-template-columns:1fr 1fr}}@media(max-width:560px){.metrics,.unit-form{grid-template-columns:1fr}.head,.toolbar{align-items:flex-start;flex-direction:column}.toolbar input{max-width:none}}
  `]
})
export class PropertiesComponent implements OnChanges {
  @Input({ required: true }) token = '';
  @Output() changed = new EventEmitter<void>();
  units: Unit[] = []; query = ''; loading = false; saving = false; showForm = false; message = ''; isError = false;
  draft = { unitNumber: '', floor: 1, type: '' };
  constructor(private http: HttpClient) {}
  ngOnChanges() { if (this.token) this.load(); }
  private headers() { return new HttpHeaders({ Authorization: `Bearer ${this.token}` }); }
  get filtered() { const value = this.query.trim().toLowerCase(); return value ? this.units.filter(unit => `${unit.unitNumber} ${unit.type}`.toLowerCase().includes(value)) : this.units; }
  get activeCount() { return this.units.filter(unit => unit.isActive).length; }
  get occupiedCount() { return this.units.filter(unit => unit.residentCount > 0).length; }
  get vacantCount() { return this.units.filter(unit => unit.residentCount === 0).length; }
  load() { this.loading = true; this.http.get<Unit[]>('http://localhost:5112/api/admin/properties/units', { headers: this.headers() }).subscribe({ next: units => { this.units = units; this.loading = false; }, error: () => { this.message = 'Unable to load units.'; this.isError = true; this.loading = false; } }); }
  add() { this.saving = true; this.message = ''; this.http.post('http://localhost:5112/api/admin/properties/units', this.draft, { headers: this.headers() }).subscribe({ next: () => { this.saving = false; this.showForm = false; this.draft = { unitNumber: '', floor: 1, type: '' }; this.message = 'Unit added successfully.'; this.isError = false; this.load(); this.changed.emit(); }, error: error => { this.saving = false; this.message = error.status === 409 ? 'This unit number already exists.' : 'Unable to save unit.'; this.isError = true; } }); }
}
