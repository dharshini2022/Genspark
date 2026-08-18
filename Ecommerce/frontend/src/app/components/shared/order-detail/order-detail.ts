import { Component, OnInit, Input, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService } from '../../../services/order.service';
import { ShipmentService, ShipmentResponseDTO } from '../../../services/shipment.service';
import { OrderSummaryResponse } from '../../../models/order.model';
import { VendorService } from '../../../services/vendor.service';
import { UserService } from '../../../services/user.service';
import { ReviewService } from '../../../services/review.service';
import { ToastService } from '../../../services/toast.service';

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ResolveImagePipe],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.css',
})
export class OrderDetail implements OnInit {
  @Input() orderId!: number;

  order = signal<OrderSummaryResponse | null>(null);
  shipments = signal<ShipmentResponseDTO[]>([]);
  loading = signal<boolean>(true);
  errorMsg = signal<string>('');
  role = signal<string>('Customer');
  vendorIdVal = signal<number | null>(null);

  private lastLoadedOrderId: number | null = null;

  constructor(
    private orderService: OrderService,
    private shipmentService: ShipmentService,
    private vendorService: VendorService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private userService: UserService,
    private reviewService: ReviewService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.role.set(sessionStorage.getItem('role') || 'Customer');

    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        this.orderId = Number(idStr);
      }

      if (this.role() === 'Vendor') {
        this.vendorService.getMyVendorProfile().subscribe({
          next: (profile) => {
            this.vendorIdVal.set(profile.id);
            if (this.orderId) {
              this.loadOrderDetail();
            }
          },
          error: (err) => {
            console.error('Error fetching vendor profile', err);
            if (this.orderId) {
              this.loadOrderDetail();
            }
          }
        });
      } else {
        if (this.orderId) {
          this.loadOrderDetail();
        }
      }
    });
  }

  loadOrderDetail() {
    if (this.lastLoadedOrderId === this.orderId) {
      return;
    }
    this.lastLoadedOrderId = this.orderId;

    this.loading.set(true);
    this.errorMsg.set('');

    this.orderService.getOrderDetail(this.orderId).subscribe({
      next: (data) => {
        if (this.role() === 'Vendor' && this.vendorIdVal() !== null) {
          const vId = this.vendorIdVal()!;
          if (data && data.items) {
            data.items = data.items.filter(item => item.vendorId === vId);
          }
        }
        this.order.set(data);
        this.loadShipments();
      },
      error: (err) => {
        console.error('Error fetching order details', err);
        this.errorMsg.set('Failed to load order details.');
        this.loading.set(false);
        this.lastLoadedOrderId = null;
      }
    });
  }

  loadShipments() {
    this.shipmentService.getShipmentsByOrderId(this.orderId).subscribe({
      next: (data) => {
        let filtered = data || [];
        if (this.role() === 'Vendor' && this.vendorIdVal() !== null) {
          const vId = this.vendorIdVal()!;
          filtered = filtered.filter(ship =>
            ship.orderItems && ship.orderItems.some(item => item.vendorId === vId)
          ).map(ship => {
            return {
              ...ship,
              orderItems: ship.orderItems.filter(item => item.vendorId === vId)
            };
          });
        }
        this.shipments.set(filtered);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching shipments', err);
        this.loading.set(false);
      }
    });
  }

  goBack() {
    this.location.back();
  }

  isCustomer(): boolean {
    return this.role() === 'Customer';
  }

  navigateToReview(productId: number) {
    this.userService.getProfile().subscribe({
      next: (profile) => {
        const userId = profile.id;
        this.reviewService.getReviewByUserAndProduct(userId, productId).subscribe({
          next: (review) => {
            if (review) {
              this.toastService.info('You have already reviewed the product');
              this.router.navigate(['/customer-home/product-review-form'], {
                queryParams: {
                  productId: productId,
                  orderId: this.orderId,
                  reviewId: review.id
                }
              });
            } else {
              this.router.navigate(['/customer-home/product-review-form'], {
                queryParams: {
                  productId: productId,
                  orderId: this.orderId
                }
              });
            }
          },
          error: (err) => {
            console.error('Error checking review', err);
            this.router.navigate(['/customer-home/product-review-form'], {
              queryParams: {
                productId: productId,
                orderId: this.orderId
              }
            });
          }
        });
      },
      error: (err) => {
        console.error('Error getting profile', err);
        this.router.navigate(['/customer-home/product-review-form'], {
          queryParams: {
            productId: productId,
            orderId: this.orderId
          }
        });
      }
    });
  }

  downloadInvoice() {
    alert(`Generating invoice for Order #ORD-${this.orderId}...`);
    window.print();
  }

  getOrderStatusLabel(status: any): string {
    if (typeof status === 'number') {
      const labels = [
        'PENDING PAYMENT',
        'PAYMENT FAILED',
        'CONFIRMED',
        'SHIPPED',
        'DELIVERED',
        'CANCELLED',
        'RETURN REQUESTED',
        'RETURNED',
        'PARTIAL RETURN REQUESTED',
        'PARTIALLY RETURNED'
      ];
      return labels[status] || 'PENDING';
    }
    return String(status).toUpperCase();
  }

  getOrderStatusClass(status: any): string {
    const label = this.getOrderStatusLabel(status).toLowerCase();
    if (label.includes('delivered')) return 'status-delivered';
    if (label.includes('shipped') || label.includes('confirmed')) return 'status-shipped';
    if (label.includes('cancelled') || label.includes('failed')) return 'status-cancelled';
    return 'status-confirmed';
  }

  getPaymentStatusLabel(status: any): string {
    if (typeof status === 'number') {
      const labels = ['PENDING', 'PAID', 'FAILED', 'REFUNDED'];
      return labels[status] || 'PENDING';
    }
    return String(status).toUpperCase();
  }

  getShipmentStatusLabel(status: any): string {
    if (typeof status === 'number') {
      const labels = ['PENDING', 'INITIATED', 'DELIVERED', 'PICKED', 'CANCELLED'];
      return labels[status - 1] || 'PENDING';
    }
    return String(status).toUpperCase();
  }

  getShipmentStatusClass(status: any): string {
    const label = this.getShipmentStatusLabel(status).toLowerCase();
    if (label.includes('delivered') || label.includes('picked')) return 'status-delivered';
    if (label.includes('initiated')) return 'status-shipped';
    if (label.includes('cancelled')) return 'status-cancelled';
    return 'status-pending';
  }

  getProductDetailRoute(productId: number): string[] {
    const role = this.role().toLowerCase();
    if (role === 'admin') {
      return ['/admin-home/product-detail', productId.toString()];
    } else if (role === 'vendor') {
      return ['/vendor-home/product-detail', productId.toString()];
    } else {
      return ['/customer-home/product-detail', productId.toString()];
    }
  }
}
