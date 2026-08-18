import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable, BehaviorSubject, tap, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = environment.baseUrl + '/Auth';
  
  private userSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {
    const cachedUser = sessionStorage.getItem('user');
    if (cachedUser) {
      try {
        const user = JSON.parse(cachedUser);
        this.userSubject.next(user);
      } catch {
        sessionStorage.removeItem('user');
      }
    }
  }

  get currentUserValue(): any {
    return this.userSubject.value;
  }

  decodeToken(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch {
      try {
        const parts = token.split('.');
        if (parts.length === 3) {
          return JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
        }
      } catch {
        return null;
      }
      return null;
    }
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, data);
  }

  sendForgotPasswordOtp(email: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/forgot-password/send-otp`, { email });
  }

  verifyForgotPasswordOtp(email: string, otp: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/forgot-password/verify-otp`, { email, otp });
  }

  resetPasswordWithOtp(email: string, otp: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/forgot-password/reset-password`, { email, otp, newPassword });
  }

  fetchCurrentUserDetails(): Observable<any> {
    return this.http.get<any>(`${environment.baseUrl}/User/my-token-payload`).pipe(
      tap(profile => {
        const user = {
          id: profile.id,
          fullName: profile.fullName,
          role: profile.role,
          email: profile.email
        };
        this.userSubject.next(user);
        sessionStorage.setItem('user', JSON.stringify(user));
        sessionStorage.setItem('role', user.role); 
        sessionStorage.setItem('name', user.fullName); 
      })
    );
  }

  login(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, data).pipe(
      tap((response: any) => {
        sessionStorage.removeItem('cart_count');
        sessionStorage.removeItem('wishlist_count');
        if (response && response.role) {
          sessionStorage.setItem('role', response.role);
          this.setTokens(response.accessToken, response.refreshToken, response.role);
        }
      }),
      switchMap(() => {
        return this.fetchCurrentUserDetails();
      })
    );
  }

  logout(data: any = {}): Observable<any> {
    const user = this.currentUserValue;
    const role = sessionStorage.getItem('role') || '';
    const body = {
      refreshToken: this.getRefreshToken() || '',
      userId: user?.id || 0,
      ...data
    };
    return this.http.post(`${this.baseUrl}/logout`, body).pipe(
      tap(() => {
        this.userSubject.next(null);
        sessionStorage.removeItem('user');
        sessionStorage.removeItem('role');
        sessionStorage.removeItem('name');
        sessionStorage.removeItem('cart_count');
        sessionStorage.removeItem('wishlist_count');
        this.clearTokens(role);
      })
    );
  }

  refreshToken(): Observable<any> {
    const expiredAccessToken = this.getAccessToken() || '';
    const refreshToken = this.getRefreshToken() || '';
    return this.http.post(`${this.baseUrl}/refresh`, { expiredAccessToken, refreshToken }).pipe(
      tap((response: any) => {
        if (response && response.role) {
          this.setTokens(response.accessToken, response.refreshToken, response.role);
        }
      })
    );
  }

  getAccessToken(): string | null {
    const role = sessionStorage.getItem('role') || '';
    const key = this.getAccessTokenKey(role);
    let token = localStorage.getItem(key);
    if (!token) {
      const legacyKey = this.getLegacyTokenKey(role);
      token = localStorage.getItem(legacyKey);
    }
    return token;
  }

  getRefreshToken(): string | null {
    const role = sessionStorage.getItem('role') || '';
    const key = this.getRefreshTokenKey(role);
    let token = localStorage.getItem(key);
    if (!token) {
      token = localStorage.getItem('refresh_token');
    }
    return token;
  }

  setTokens(accessToken: string, refreshToken: string, role: string): void {
    const accessKey = this.getAccessTokenKey(role);
    const refreshKey = this.getRefreshTokenKey(role);
    localStorage.setItem(accessKey, accessToken);
    localStorage.setItem(refreshKey, refreshToken);
  }

  clearTokens(role?: string): void {
    const r = role || sessionStorage.getItem('role') || '';
    if (r) {
      localStorage.removeItem(this.getAccessTokenKey(r));
      localStorage.removeItem(this.getRefreshTokenKey(r));
    } else {
      localStorage.removeItem('customer_access_token');
      localStorage.removeItem('customer_refresh_token');
      localStorage.removeItem('vendor_access_token');
      localStorage.removeItem('vendor_refresh_token');
      localStorage.removeItem('admin_access_token');
      localStorage.removeItem('admin_refresh_token');
      localStorage.removeItem('customer_token');
      localStorage.removeItem('vendor_token');
      localStorage.removeItem('admin_token');
      localStorage.removeItem('refresh_token');
    }
  }

  private getAccessTokenKey(role: string): string {
    const r = role?.toLowerCase() || '';
    if (r === 'customer') return 'customer_access_token';
    if (r === 'vendor') return 'vendor_access_token';
    if (r === 'admin') return 'admin_access_token';
    return 'access_token';
  }

  private getRefreshTokenKey(role: string): string {
    const r = role?.toLowerCase() || '';
    if (r === 'customer') return 'customer_refresh_token';
    if (r === 'vendor') return 'vendor_refresh_token';
    if (r === 'admin') return 'admin_refresh_token';
    return 'refresh_token';
  }

  private getLegacyTokenKey(role: string): string {
    const r = role?.toLowerCase() || '';
    if (r === 'customer') return 'customer_token';
    if (r === 'vendor') return 'vendor_token';
    if (r === 'admin') return 'admin_token';
    return 'jwt_token';
  }

  updateCurrentUser(fullName: string): void {
    const cachedUser = sessionStorage.getItem('user');
    if (cachedUser) {
      try {
        const user = JSON.parse(cachedUser);
        user.fullName = fullName;
        sessionStorage.setItem('user', JSON.stringify(user));
        this.userSubject.next(user);
      } catch (e) {
        console.error('Error parsing cached user for name update:', e);
      }
    }
  }
}
