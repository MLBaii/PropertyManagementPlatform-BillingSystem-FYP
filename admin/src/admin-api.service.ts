import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminBill, AdminSession, AgingSummary, DashboardSummary } from './admin-types';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly baseUrl = 'http://localhost:5112/api/admin';
  constructor(private readonly http: HttpClient) {}
  login(username: string, password: string): Observable<AdminSession> { return this.http.post<AdminSession>('http://localhost:5112/api/auth/admin/login', { username, password }); }
  dashboard(token: string): Observable<DashboardSummary> { return this.http.get<DashboardSummary>(`${this.baseUrl}/dashboard`, { headers: this.headers(token) }); }
  bills(token: string): Observable<AdminBill[]> { return this.http.get<AdminBill[]>(`${this.baseUrl}/bills`, { headers: this.headers(token) }); }
  agingSummary(token: string): Observable<AgingSummary> { return this.http.get<AgingSummary>(`${this.baseUrl}/reports/aging-summary`, { headers: this.headers(token) }); }
  private headers(token: string): HttpHeaders { return new HttpHeaders({ Authorization: `Bearer ${token}` }); }
}
