import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { AuthResponse, LoginDto, OperatorRegisterDto, RegisterDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = '/api/auth';
  //Behaviour Subject is used to store the current user state and allow components to subscribe to changes in the authentication state.
  private userSubject = new BehaviorSubject<AuthResponse | null>(this.loadUser());
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}
  //gets the user details from the local storage
  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem('bbs_user');
    return raw ? JSON.parse(raw) : null;
  }
  //To check if the user is logged in. To get the token and role of the current user.
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

  //Adds the user to the local storage and updates the userSubject with the new user details. 
  login(dto: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API}/login`, dto).pipe(
      tap(res => this.saveUser(res))
    );
  }

  //Removes the user detailes from the local storage
  logout(): void {
    localStorage.removeItem('bbs_user');
    this.userSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  //Save User adds the user details to the local storage.
  private saveUser(res: AuthResponse): void {
    localStorage.setItem('bbs_user', JSON.stringify(res));
    this.userSubject.next(res);
  }
}
