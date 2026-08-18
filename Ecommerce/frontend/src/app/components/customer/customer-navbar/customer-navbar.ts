import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { CartService } from '../../../services/cart.service';
import { WishlistService } from '../../../services/wishlist.service';
import { NotificationService } from '../../../services/notification.service';
import { NotificationSidebar } from '../../shared/notification-sidebar/notification-sidebar';

@Component({
  selector: 'app-customer-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, NotificationSidebar],
  templateUrl: './customer-navbar.html',
  styleUrl: './customer-navbar.css',
})
export class CustomerNavbar {
  name = signal("Guest");
  email = signal("");
  isDropdownOpen = signal(false);
  isLoggedIn = sessionStorage.getItem("user")?  true : false;
  searchQuery = '';
  cartCount = signal<number>(0);
  wishlistCount = signal<number>(0);
  isNotificationSidebarOpen = signal(false);

  constructor(
    private elementRef: ElementRef, 
    private authService: AuthService, 
    private router: Router,
    private cartService: CartService,
    private wishlistService: WishlistService,
    public notificationService: NotificationService
  ) {
    this.cartCount = this.cartService.cartCountSignal;
    this.wishlistCount = this.wishlistService.wishlistCountSignal;
    
    this.authService.currentUser$.subscribe({
      next: (user: any) => {
        const n = user?.fullName;
        this.name.set(n ?? "Guest");
        this.email.set(user?.email ?? "");
        if (user && sessionStorage.getItem("user")) {
          if (sessionStorage.getItem('cart_count') === null) {
            this.cartService.updateCartCount();
          }
          if (sessionStorage.getItem('wishlist_count') === null || this.wishlistService.wishlistItemsSignal().length === 0) {
            this.wishlistService.updateWishlistCount();
          }
          this.notificationService.startConnection();
          this.notificationService.loadNotifications();
        } else {
          this.cartService.cartCountSignal.set(0);
          this.wishlistService.wishlistCountSignal.set(0);
          sessionStorage.removeItem('cart_count');
          sessionStorage.removeItem('wishlist_count');
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



  onSearch(): void {
    const query = this.searchQuery.trim();
    if (query) {
      this.router.navigate(['/customer-home/products-list'], {
        queryParams: { search: query }
      });
    } else {
      this.router.navigate(['/customer-home/products-list']);
    }
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
