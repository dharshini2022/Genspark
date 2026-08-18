import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { UserAddress } from '../models/address.model';
import { UserProfileResponse, UserProfileRequest, ChangePasswordRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private baseUrl = `${environment.baseUrl}/User`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileResponse> {
    return this.http.get<UserProfileResponse>(`${this.baseUrl}/profile`);
  }

  updateProfile(request: UserProfileRequest): Observable<UserProfileResponse> {
    return this.http.put<UserProfileResponse>(`${this.baseUrl}/updateProfile`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/changePassword`, request);
  }

  getMyAddresses(): Observable<UserAddress[]> {
    return this.http.get<UserAddress[]>(`${this.baseUrl}/GetMyAddress`);
  }

  addUserAddress(address: UserAddress): Observable<UserAddress> {
    return this.http.post<UserAddress>(`${this.baseUrl}/UserAddress`, address);
  }

  updateUserAddress(id: number, address: UserAddress): Observable<UserAddress> {
    return this.http.put<UserAddress>(`${this.baseUrl}/UserAddress/${id}`, address);
  }

  toggleAccountStatus(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/ToggleAccount`, {});
  }
}
