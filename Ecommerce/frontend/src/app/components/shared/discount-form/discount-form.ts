import { Component, Output, EventEmitter, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { CreateDiscountRequest } from '../../../models/disocunt.model';
import { ProductSearchResult } from '../../../models/product.model';
import { Category } from '../../../models/category.model';

@Component({
  selector: 'app-discount-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './discount-form.html',
  styleUrl: './discount-form.css',
})
export class DiscountForm implements OnInit {
  @Output() cancel = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();

  constructor(
    private discountService: DiscountService,
    private toastService: ToastService,
    private productService: ProductService,
    private categoryService: CategoryService
  ) {}

  submittingDiscount = signal<boolean>(false);
  isAdmin = sessionStorage.getItem('role') === 'Admin';

  selectedScope = signal<string>(this.isAdmin ? 'Common' : 'Vendor');
  newDiscountType = signal<'Flat' | 'Percentage'>('Flat');
  newDiscountValue = signal<number | null>(null);
  newDiscountMinOrder = signal<number | null>(null);
  newDiscountUsageLimit = signal<number>(10);
  newDiscountExpiry = signal<string>('');

  categories = signal<Category[]>([]);
  selectedCategoryId = signal<number | null>(null);

  productSearchQuery = signal<string>('');
  searchedProducts = signal<ProductSearchResult[]>([]);
  selectedProduct = signal<ProductSearchResult | null>(null);

  ngOnInit() {
  }

  onScopeChange(scope: string) {
    this.selectedScope.set(scope);
    if (scope === 'Category' && this.categories().length === 0) {
      this.loadCategories();
    }
  }

  loadCategories() {
    this.categoryService.getCategories().subscribe({
      next: (res) => {
        this.categories.set(res || []);
      },
      error: (err) => {
        console.error('Failed to load categories', err);
      }
    });
  }

  onProductSearchChange(query: string) {
    this.productSearchQuery.set(query);
    if (!query.trim()) {
      this.searchedProducts.set([]);
      return;
    }
    this.productService.search(query).subscribe({
      next: (res) => {
        this.searchedProducts.set(res || []);
      },
      error: (err) => {
        console.error('Product search failed', err);
      }
    });
  }

  selectProduct(product: ProductSearchResult) {
    this.selectedProduct.set(product);
    this.productSearchQuery.set(product.name);
    this.searchedProducts.set([]);
  }

  cancelAddDiscount() {
    this.cancel.emit();
  }

  submitDiscount(event: Event) {
    event.preventDefault();

    const val = this.newDiscountValue();
    const type = this.newDiscountType();
    const minOrder = this.newDiscountMinOrder();
    const limit = this.newDiscountUsageLimit();
    const expiry = this.newDiscountExpiry();
    const scope = this.selectedScope();

    if (!val || val <= 0) {
      this.toastService.error('Discount value must be greater than 0.');
      return;
    }
    if (type === 'Percentage' && val > 100) {
      this.toastService.error('Percentage discount cannot exceed 100%.');
      return;
    }
    if (minOrder === null || minOrder < 0) {
      this.toastService.error('Minimum order value cannot be negative.');
      return;
    }
    if (type === 'Flat' && val > minOrder) {
      this.toastService.error('Flat discount value cannot exceed minimum order value.');
      return;
    }
    if (!limit || limit < 10) {
      this.toastService.error('Usage limit must be at least 10.');
      return;
    }
    if (!expiry) {
      this.toastService.error('Expiry date is required.');
      return;
    }

    if (scope === 'Product' && !this.selectedProduct()) {
      this.toastService.error('Please select a product for the Product-scoped discount.');
      return;
    }

    if (scope === 'Category' && !this.selectedCategoryId()) {
      this.toastService.error('Please select a category for the Category-scoped discount.');
      return;
    }

    const expiryDate = new Date(expiry);
    if (expiryDate <= new Date()) {
      this.toastService.error('Expiry date must be in the future.');
      return;
    }

    this.submittingDiscount.set(true);
    const request: CreateDiscountRequest = {
      scope: scope,
      type: type,
      value: val,
      minOrderValue: minOrder,
      usageLimit: limit,
      expiresAt: expiryDate.toISOString(),
      productId: scope === 'Product' ? this.selectedProduct()?.id : undefined,
      categoryId: scope === 'Category' ? Number(this.selectedCategoryId()) : undefined
    };

    this.discountService.createDiscount(request).subscribe({
      next: () => {
        this.toastService.success('Discount coupon created successfully!');
        this.submittingDiscount.set(false);
        this.success.emit();
      },
      error: (err) => {
        console.error('Error creating discount', err);
        this.toastService.error(err.error?.message || 'Failed to create discount.');
        this.submittingDiscount.set(false);
      }
    });
  }
}
