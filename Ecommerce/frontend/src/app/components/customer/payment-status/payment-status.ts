import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CartService } from '../../../services/cart.service';
import { OrderService } from '../../../services/order.service';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-payment-status',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './payment-status.html',
  styleUrl: './payment-status.css'
})
export class PaymentStatus implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private toastService = inject(ToastService);

  isLoading = signal(true);
  isSuccess = signal(false);
  errorMsg = signal('Your payment could not be processed. Please try again.');

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    const status = params['redirect_status'] || params['status'];
    const orderId = Number(params['orderId']);

    if ((status === 'succeeded' || status === 'success') && orderId) {
      this.orderService.makePayment(orderId, 'stripe_checkout').subscribe({
        next: () => {
          this.isSuccess.set(true);
          this.cartService.cartCountSignal.set(0);
          this.toastService.success('Payment confirmed! Your order is placed.');
          setTimeout(() => this.goToOrders(), 3000);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isSuccess.set(false);
          this.errorMsg.set(err.error?.message || 'Failed to update order status.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.isSuccess.set(false);
      this.errorMsg.set('Payment was not completed. Status: ' + (status || 'unknown'));
      this.isLoading.set(false);
    }
  }

  goToOrders(): void {
    this.router.navigate(['/customer-home/order-list']);
  }

  goBack(): void {
    this.router.navigate(['/customer-home/cart']);
  }
}
