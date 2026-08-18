import { CommonModule } from '@angular/common';
import { Component, signal, HostListener, ElementRef, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';
import { NotificationSidebar } from '../../shared/notification-sidebar/notification-sidebar';

@Component({
  selector: 'app-vendor-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, NotificationSidebar],
  templateUrl: './vendor-navbar.html',
  styleUrl: './vendor-navbar.css',
})
export class VendorNavbar {
  name = signal("Vendor");
  role: string = "Vendor";
  isDropdownOpen = signal(false);
  isNotificationSidebarOpen = signal(false);

  constructor(
    private elementRef: ElementRef,
    private authService: AuthService,
    private router: Router,
    public notificationService: NotificationService
  ) {
    this.authService.currentUser$.subscribe({
      next: (user: any) => {
        const n = user?.fullName;
        this.name.set(n ?? "Vendor");
        if (user && sessionStorage.getItem("user")) {
          this.notificationService.startConnection();
          this.notificationService.loadNotifications();
        } else {
          this.notificationService.stopConnection();
          this.notificationService.notifications.set([]);
          this.notificationService.unreadCount.set(0);
        }
      }
    });
  }

  toggleDropdown(event: MouseEvent): void {
    event.stopPropagation();
    this.isDropdownOpen.update(val => !val);
  }

  closeDropdown(): void {
    this.isDropdownOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  toggleNotificationSidebar(event: MouseEvent): void {
    event.stopPropagation();
    this.isNotificationSidebarOpen.update(val => !val);
  }

  closeNotificationSidebar(): void {
    this.isNotificationSidebarOpen.set(false);
  }

  logout(): void {
    this.closeDropdown();
    this.authService.logout({}).subscribe({
      next: () => {
        this.notificationService.stopConnection();
        this.notificationService.notifications.set([]);
        this.notificationService.unreadCount.set(0);
        sessionStorage.clear();
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error("Logout failed, clearing session locally:", err);
        this.notificationService.stopConnection();
        this.notificationService.notifications.set([]);
        this.notificationService.unreadCount.set(0);
        sessionStorage.clear();
        this.router.navigate(['/login']);
      }
    });
  }
}
