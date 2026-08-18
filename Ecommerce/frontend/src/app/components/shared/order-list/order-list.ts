import { Component, OnInit, OnChanges, Input, SimpleChanges, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { OrderService } from '../../../services/order.service';
import { OrderSummaryResponse } from '../../../models/order.model';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './order-list.html',
  styleUrl: './order-list.css'
})
export class OrderList implements OnInit, OnChanges {
  @Input() role: 'Vendor' | 'Admin' | 'Customer' = 'Vendor';
  @Input() vendorId?: number;
  @Output() totalOrdersCount = new EventEmitter<number>();

  orders = signal<OrderSummaryResponse[]>([]);
  page = signal<number>(1);
  pageSize = 10;
  totalCount = signal<number>(0);
  totalPages = signal<number>(0);
  loading = signal<boolean>(false);
  errorMsg = signal<string>('');

  constructor(
    private orderService: OrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  viewOrderDetail(orderId: number) {
    if (this.role === 'Admin') {
      this.router.navigate(['/admin-home/order-detail', orderId]);
    } else if (this.role === 'Vendor') {
      this.router.navigate(['/vendor-home/order-detail', orderId]);
    } else {
      this.router.navigate(['/customer-home/order-detail', orderId]);
    }
  }

  ngOnInit() {
    this.route.data.subscribe(data => {
      if (data && data['role']) {
        this.role = data['role'];
      } else {
        const storedRole = sessionStorage.getItem('role');
        if (storedRole === 'Admin') this.role = 'Admin';
        else if (storedRole === 'Vendor') this.role = 'Vendor';
        else if (storedRole === 'Customer') this.role = 'Customer';
      }
      this.loadOrders();
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['role'] || changes['vendorId']) {
      this.page.set(1);
      this.loadOrders();
    }
  }

  loadOrders() {
    const loggedInRole = sessionStorage.getItem('role');

    this.loading.set(true);
    this.errorMsg.set('');

    const pageNum = this.page();
    const size = this.pageSize;

    let obs$;
    if (this.role === 'Admin') {
      obs$ = this.orderService.getAllOrders(pageNum, size);
    } else if (this.role === 'Vendor') {
      obs$ = this.orderService.getVendorOrders(pageNum, size);
    } else {
      obs$ = this.orderService.getMyOrders(pageNum, size);
    }

    obs$.pipe(
      catchError(err => {
        console.error('Error loading orders', err);
        this.errorMsg.set('Failed to load orders.');
        this.orders.set([]);
        this.totalCount.set(0);
        this.totalPages.set(0);
        return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 5, totalPages: 0, hasNext: false, hasPrevious: false });
      })
    ).subscribe(res => {
      if (res) {
        this.orders.set(res.items || []);
        this.totalCount.set(res.totalCount || 0);
        this.totalOrdersCount.emit(res.totalCount || 0);
        const calculatedPages = Math.ceil((res.totalCount || 0) / this.pageSize);
        this.totalPages.set(res.totalPages || calculatedPages || 1);
      }
      this.loading.set(false);
    });
  }

  setPage(pageNum: number) {
    if (pageNum >= 1 && pageNum <= this.totalPages()) {
      this.page.set(pageNum);
      this.loadOrders();
    }
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

  getPaymentStatusClass(status: any): string {
    const label = this.getPaymentStatusLabel(status).toLowerCase();
    if (label.includes('paid')) return 'badge-active';
    if (label.includes('refunded')) return 'badge-draft';
    if (label.includes('failed')) return 'status-cancelled';
    return 'status-confirmed';
  }
}
