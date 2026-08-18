import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

export enum NotificationType {
  OrderPlaced = 1,
  OrderShipped = 2,
  OrderDelivered = 3,
  PaymentFailed = 4,
  PriceDrop = 5,
  FlashSale = 6,
  ReviewApproved = 7,
  VendorApproved = 8,
  VendorPending = 9
}

export enum NotificationLevel {
  Info = 1,
  Success = 2,
  Warning = 3,
  Error = 4
}

export interface Notification {
  id: number;
  userId: number;
  type: NotificationType | string;
  level: NotificationLevel | string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private baseUrl = environment.baseUrl + '/Notification';
  private hubConnection?: signalR.HubConnection;

  notifications = signal<Notification[]>([]);
  unreadCount = signal<number>(0);

  constructor(private http: HttpClient, private authService: AuthService) {}

  loadNotifications(): void {
    this.http.get<Notification[]>(this.baseUrl).subscribe({
      next: (data) => {
        const role = sessionStorage.getItem('role') || 'Customer';
        const filtered = data.filter(n => {
          const isVendorPending = n.type === NotificationType.VendorPending || 
                                  n.type === 'VendorPending' || 
                                  Number(n.type) === 9;
          if (isVendorPending && role !== 'Admin') {
            return false;
          }
          return true;
        });
        this.notifications.set(filtered);
        this.updateUnreadCount();
      },
      error: (err) => console.error('Failed to load notifications', err)
    });
  }

  markAllAsRead(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/mark-all-read`, {});
  }

  markAsRead(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/${id}/read`, {});
  }

  private updateUnreadCount(): void {
    const unread = this.notifications().filter(n => !n.isRead).length;
    this.unreadCount.set(unread);
  }

  startConnection(): void {
    const cachedUser = sessionStorage.getItem('user');
    if (!cachedUser) return;

    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const hubUrl = environment.baseUrl.replace('/api', '/notificationHub');
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authService.getAccessToken() || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR connected to NotificationHub'))
      .catch(err => console.error('SignalR connection error: ', err));

    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      const isVendorPending = notification.type === NotificationType.VendorPending || 
                              notification.type === 'VendorPending' || 
                              Number(notification.type) === 9;
      const role = sessionStorage.getItem('role') || 'Customer';
      if (isVendorPending && role !== 'Admin') {
        return;
      }

      this.notifications.update(prev => [notification, ...prev]);
      this.unreadCount.update(count => count + 1);
    });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR connection stopped'))
        .catch(err => console.error('SignalR stop error: ', err));
      this.hubConnection = undefined;
    }
  }
}
