import { Component, OnInit, signal, computed, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { DiscountResponse, CreateDiscountRequest } from '../../../models/disocunt.model';
import { DiscountForm } from '../discount-form/discount-form';

@Component({
  selector: 'app-discount-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DiscountForm],
  templateUrl: './discount-list.html',
  styleUrl: './discount-list.css'
})
export class DiscountList implements OnInit {
  @Input() vendorId: number | null = null;
  @Input() isAdminView: boolean = false;

  constructor(
    private discountService: DiscountService,
    private toastService: ToastService
  ) {}

  discounts = signal<DiscountResponse[]>([]);
  loading = signal<boolean>(false);
  
  showAddDiscountForm = signal<boolean>(false);

  page = signal<number>(1);
  pageSize = signal<number>(10);
  totalCount = signal<number>(0);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  ngOnInit() {
    if (sessionStorage.getItem('role') === 'Admin') {
      this.isAdminView = true;
    }
    this.loadDiscounts();
  }

  loadDiscounts() {
    this.loading.set(true);
    const pNum = this.page();
    const pSize = this.pageSize();

    const request$ = this.isAdminView
      ? (this.vendorId 
          ? this.discountService.getVendorDiscountsByAdmin(this.vendorId, pNum, pSize)
          : this.discountService.getDiscountHisotry({ pageNumber: pNum, pageSize: pSize }))
      : this.discountService.getMyVendorDiscounts(pNum, pSize);

    request$.subscribe({
      next: (res) => {
        this.discounts.set(res.items || []);
        this.totalCount.set(res.totalCount || 0);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading discounts', err);
        this.toastService.error('Failed to load discounts list.');
        this.loading.set(false);
      }
    });
  }

  setPage(pageNum: number) {
    if (pageNum >= 1 && pageNum <= this.totalPages()) {
      this.page.set(pageNum);
      this.loadDiscounts();
    }
  }

  addDiscount() {
    this.showAddDiscountForm.set(true);
  }

  onFormCancel() {
    this.showAddDiscountForm.set(false);
  }

  onFormSuccess() {
    this.showAddDiscountForm.set(false);
    this.loadDiscounts();
  }

  deactivateDiscount(discountCode: string) {
    if (confirm(`Deactivate discount coupon "${discountCode}"?`)) {
      this.discountService.deactivateDiscount(discountCode).subscribe({
        next: () => {
          this.toastService.success(`Discount coupon "${discountCode}" deactivated.`);
          this.loadDiscounts();
        },
        error: (err) => {
          console.error('Error deactivating discount', err);
          this.toastService.error('Failed to deactivate coupon.');
        }
      });
    }
  }
}
