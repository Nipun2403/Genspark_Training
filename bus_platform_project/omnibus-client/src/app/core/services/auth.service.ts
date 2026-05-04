import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, AuthResponse } from '../../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const stored = localStorage.getItem('omnibus_user');
    if (stored) {
      this.currentUserSubject.next(JSON.parse(stored));
    }
  }

  get currentUser(): AuthResponse | null { return this.currentUserSubject.value; }
  get isLoggedIn(): boolean { return !!this.currentUser?.token; }
  get token(): string | null { return this.currentUser?.token ?? null; }
  get role(): string | null { return this.currentUser?.role ?? null; }
  get userId(): string | null { return this.currentUser?.userId ?? null; }

  sendOtp(email: string): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/send-otp`, { email });
  }

  verifyOtp(email: string, code: string): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/verify-otp`, { email, code }).pipe(
      tap(res => {
        if (res.success && res.data) {
          localStorage.setItem('omnibus_user', JSON.stringify(res.data));
          localStorage.setItem('omnibus_token', res.data.token);
          this.currentUserSubject.next(res.data);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('omnibus_user');
    localStorage.removeItem('omnibus_token');
    this.currentUserSubject.next(null);
  }
}
