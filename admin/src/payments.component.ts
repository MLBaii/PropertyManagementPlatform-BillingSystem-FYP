import { DatePipe } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, Input, OnChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface PaymentProof {
  proofId: number;
  residentName: string;
  unitNumber: string;
  fileUrl: string;
  fileType: string;
  fileSize: number;
  status: 'Pending' | 'Confirmed' | 'Rejected';
  submittedAt: string;
  adminRemarks?: string;
}

@Component({
  selector: 'app-payments',
  standalone: true,
  imports: [FormsModule, DatePipe],
  template: `
    <section class="panel">
      <div class="head">
        <div><h2>Payment proof reviews</h2><p>Confirm or reject resident payment submissions.</p></div>
        <button (click)="load()" [disabled]="loading">{{ loading ? 'Refreshing...' : 'Refresh' }}</button>
      </div>

      @if (message) { <p class="message" [class.error]="isError">{{ message }}</p> }
      @if (loading) { <p class="empty">Loading payment submissions...</p> }
      @else if (proofs.length === 0) { <p class="empty">No payment proofs have been submitted yet.</p> }
      @else {
        <div class="table-wrap"><table>
          <thead><tr><th>Resident</th><th>Unit</th><th>Submitted</th><th>Proof</th><th>Status</th><th>Review</th></tr></thead>
          <tbody>@for (proof of proofs; track proof.proofId) {
            <tr>
              <td>{{ proof.residentName }}</td><td>{{ proof.unitNumber }}</td><td>{{ proof.submittedAt | date:'mediumDate' }}</td>
              <td><a [href]="proof.fileUrl" target="_blank" rel="noopener">Open proof</a></td>
              <td><span class="status" [class.confirmed]="proof.status === 'Confirmed'" [class.rejected]="proof.status === 'Rejected'">{{ proof.status }}</span></td>
              <td>
                @if (proof.status === 'Pending') {
                  <div class="review"><input [(ngModel)]="remarks[proof.proofId]" maxlength="500" placeholder="Optional remarks"><button class="confirm" (click)="review(proof, 'Confirmed')" [disabled]="reviewing === proof.proofId">Confirm</button><button class="reject" (click)="review(proof, 'Rejected')" [disabled]="reviewing === proof.proofId">Reject</button></div>
                } @else { <span class="remarks">{{ proof.adminRemarks || 'Reviewed' }}</span> }
              </td>
            </tr>
          }</tbody>
        </table></div>
      }
    </section>`,
  styles: [`
    .panel{margin-top:28px;padding:22px;background:#fff;border:1px solid #e4ddd7;border-radius:12px}.head{display:flex;justify-content:space-between;gap:16px;align-items:center}.head h2{margin:0 0 6px}.head p,.empty,.remarks{margin:0;color:#736963}.message{font-weight:700;color:#276738}.error{color:#b42318}.table-wrap{overflow:auto}table{width:100%;border-collapse:collapse;margin-top:18px;min-width:860px}th,td{text-align:left;padding:13px 10px;border-top:1px solid #e9e2dc;vertical-align:middle}th{color:#736963;font-size:.8rem;text-transform:uppercase}a{color:#a84d2e;font-weight:700}.status{display:inline-block;padding:4px 8px;border-radius:9px;background:#fff2d7;color:#896417;font-size:.85rem}.confirmed{background:#e7f4ea;color:#276738}.rejected{background:#fde4da;color:#9c2b18}.review{display:flex;gap:7px;align-items:center}.review input{min-width:150px;padding:8px;border:1px solid #d8cec7;border-radius:6px}.review button,.head button{padding:9px 12px;border:0;border-radius:7px;color:#fff;font-weight:700;cursor:pointer;background:#bd613f}.review .confirm{background:#276738}.review .reject{background:#a13d28}.review button:disabled,.head button:disabled{opacity:.6;cursor:wait}
  `]
})
export class PaymentsComponent implements OnChanges {
  @Input({ required: true }) token = '';
  proofs: PaymentProof[] = [];
  remarks: Record<number, string> = {};
  loading = false;
  reviewing: number | null = null;
  message = '';
  isError = false;

  constructor(private http: HttpClient) {}

  ngOnChanges() { if (this.token) this.load(); }
  private headers() { return new HttpHeaders({ Authorization: `Bearer ${this.token}` }); }
  load() {
    this.loading = true;
    this.http.get<PaymentProof[]>('http://localhost:5112/api/admin/payment-proofs', { headers: this.headers() }).subscribe({
      next: (data) => { this.proofs = data; this.loading = false; },
      error: () => { this.message = 'Unable to load payment submissions.'; this.isError = true; this.loading = false; }
    });
  }
  review(proof: PaymentProof, decision: 'Confirmed' | 'Rejected') {
    this.reviewing = proof.proofId;
    this.message = '';
    this.http.put(`http://localhost:5112/api/admin/payment-proofs/${proof.proofId}/review`, { decision, adminRemarks: this.remarks[proof.proofId]?.trim() || null }, { headers: this.headers() }).subscribe({
      next: () => { this.message = `Payment proof ${decision.toLowerCase()}.`; this.isError = false; this.reviewing = null; this.load(); },
      error: (error) => { this.message = error.status === 409 ? 'This proof has already been reviewed.' : 'Unable to review this payment proof.'; this.isError = true; this.reviewing = null; }
    });
  }
}
