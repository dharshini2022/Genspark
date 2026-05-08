import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, LoginDto, OperatorRegisterDto, RegisterDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = '/api/auth';
  private userSubject = new BehaviorSubject<AuthResponse | null>(this.loadUser());
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem('bbs_user');
    return raw ? JSON.parse(raw) : null;
  }

  get currentUser(): AuthResponse | null { return this.userSubject.value; }
  get isLoggedIn(): boolean { return !!this.userSubject.value; }
  get role(): string { return this.userSubject.value?.role ?? ''; }
  get token(): string { return this.userSubject.value?.token ?? ''; }

  register(dto: RegisterDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API}/register`, dto).pipe(
      tap(res => this.saveUser(res))
    );
  }

  registerOperator(dto: OperatorRegisterDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API}/operator/register`, dto).pipe(
      tap(res => this.saveUser(res))
    );
  }

  login(dto: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API}/login`, dto).pipe(
      tap(res => this.saveUser(res))
    );
  }

  logout(): void {
    localStorage.removeItem('bbs_user');
    this.userSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  private saveUser(res: AuthResponse): void {
    localStorage.setItem('bbs_user', JSON.stringify(res));
    this.userSubject.next(res);
  }
}
