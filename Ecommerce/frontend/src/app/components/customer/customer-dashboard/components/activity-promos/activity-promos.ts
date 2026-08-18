import { Component, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartService } from '../../../../../services/cart.service';
import { AuthService } from '../../../../../services/auth.service';

@Component({
  selector: 'app-activity-promos',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './activity-promos.html',
  styleUrl: './activity-promos.css',
})
export class ActivityPromosComponent implements OnInit {
  cartCount = computed(() => this.cartService.cartCountSignal());

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    if (this.isLoggedIn()) {
      this.cartService.updateCartCount();
    }
  }

  isLoggedIn(): boolean {
    return !!sessionStorage.getItem('user');
  }

  viewCart() {
    if (this.isLoggedIn()) {
      this.router.navigate(['/customer-home/cart']);
    } else {
      this.router.navigate(['/login']);
    }
  }

  viewDiscounts() {
    this.router.navigate(['/customer-home/products-list'], {
      queryParams: { sortBy: 'discount' }
    });
  }
}
