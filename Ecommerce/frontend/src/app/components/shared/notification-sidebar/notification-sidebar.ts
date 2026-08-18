import { Component, Input, Output, EventEmitter, inject, computed } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService, Notification, NotificationType } from '../../../services/notification.service';

@Component({
  selector: 'app-notification-sidebar',
  standalone: true,
  imports: [],
  templateUrl: './notification-sidebar.html',
  styleUrl: './notification-sidebar.css',
})
export class NotificationSidebar {
  @Input() isOpen = false;
  @Output() closeSidebar = new EventEmitter<void>();

  private notificationService = inject(NotificationService);
  private router = inject(Router);

  notifications = computed(() => this.notificationService.notifications().filter(n => !n.isRead));
  unreadCount = this.notificationService.unreadCount;

  onClose(): void {
    this.closeSidebar.emit();
  }

  onNotificationClick(notification: Notification): void {
    this.onMarkAsRead(notification);
    this.onClose();

    const isVendorPending = notification.type === NotificationType.VendorPending || 
                            notification.type === 'VendorPending' || 
                            Number(notification.type) === 9;

    if (isVendorPending) {
      const match = notification.message.match(/#(\d+)/);
      if (match) {
        const vendorId = match[1];
        this.router.navigate([`/admin-home/vendor-profile/${vendorId}`]);
        return;
      }
    }

    const match = notification.message.match(/#(\d+)/);
    if (match) {
      const orderId = match[1];
      const role = sessionStorage.getItem('role') || 'Customer';
      
      if (role === 'Customer') {
        this.router.navigate([`/customer-home/order-detail/${orderId}`]);
      } else if (role === 'Vendor') {
        this.router.navigate([`/vendor-home/order-detail/${orderId}`]);
      } else if (role === 'Admin') {
        this.router.navigate([`/admin-home/order-detail/${orderId}`]);
      }
    }
  }

  onMarkAsRead(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe({
        next: () => {
          this.notificationService.notifications.update(list =>
            list.map(n => n.id === notification.id ? { ...n, isRead: true } : n)
          );
          const currentUnread = this.notificationService.notifications().filter(n => !n.isRead).length;
          this.notificationService.unreadCount.set(currentUnread);
        },
        error: (err) => console.error('Failed to mark notification as read', err)
      });
    }
  }

  onMarkAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notificationService.notifications.update(list =>
          list.map(n => ({ ...n, isRead: true }))
        );
        this.notificationService.unreadCount.set(0);
      },
      error: (err) => console.error('Failed to mark all notifications as read', err)
    });
  }

  formatTime(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const today = new Date();
    
    if (date.toDateString() === today.toDateString()) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
    
    return date.toLocaleDateString([], { month: 'short', day: 'numeric' }) + ' ' + 
           date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
}
