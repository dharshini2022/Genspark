import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { timer } from 'rxjs';
import { OrderService } from '../../../services/order.service';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './payment.html',
  styleUrl: './payment.css',
})
export class Payment implements OnInit {
  private route   = inject(ActivatedRoute);
  private router  = inject(Router);
  private orderService = inject(OrderService);
  private toastService = inject(ToastService);

  orderId = signal<number | null>(null);
  amount  = signal<number | null>(null);
  isLoading    = signal(true);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const params  = this.route.snapshot.queryParams;
    const orderId = Number(params['orderId']);
    const amount  = Number(params['amount']);

    if (!orderId || isNaN(orderId)) {
      this.toastService.error('Invalid payment session. Redirecting back…');
      this.router.navigate(['/customer-home/cart']);
      return;
    }

    this.orderId.set(orderId);
    this.amount.set(amount);

    // Automatically create the Stripe Checkout Session and redirect
    this.redirectToStripe(orderId);
  }

  redirectToStripe(orderId: number): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.orderService.createCheckoutSession(orderId).subscribe({
      next: (res) => {
        window.location.href = res.url;
      },
      error: (err) => {
        const msg = err.error?.message || 'Could not start payment. Please try again.';
        this.errorMessage.set(msg);
        this.isLoading.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/customer-home/order-checkout']);
  }

  formatCurrency(amount: number | null): string {
    if (amount === null) return '—';
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 2
    }).format(amount);
  }
}
