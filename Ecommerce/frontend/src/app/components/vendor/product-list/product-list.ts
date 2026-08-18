import { Component, OnInit, signal, computed, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { ToastService } from '../../../services/toast.service';
import { CategoryService } from '../../../services/category.service';
import { ProductResponse } from '../../../models/product.model';
import { Category } from '../../../models/category.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductList implements OnInit {
  @Input() vendorId: number | null = null;
  @Input() isAdminView: boolean = false;

  private productService = inject(ProductService);
  private toastService = inject(ToastService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);

  products = signal<ProductResponse[]>([]);
  loading = signal<boolean>(false);
  categories = signal<Category[]>([]);
  
  page = signal<number>(1);
  pageSize = signal<number>(5);
  totalCount = signal<number>(0);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  showEditProductModal = signal<boolean>(false);
  editingProduct = signal<ProductResponse | null>(null);
  editProductName = '';
  editProductDescription = '';
  editProductCategoryId: number | null = null;

  ngOnInit() {
    this.loadProducts();
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats || []),
      error: (err) => console.error('Error fetching categories:', err)
    });
  }

  loadProducts() {
    this.loading.set(true);
    const pNum = this.page();
    const pSize = this.pageSize();

    const request$ = this.isAdminView && this.vendorId
      ? this.productService.getProductsByVendorId(this.vendorId, pNum, pSize)
      : this.productService.getVendorProducts(pNum, pSize);

    request$.subscribe({
      next: (res) => {
        this.products.set(res.items || []);
        this.totalCount.set(res.totalCount || 0);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading products', err);
        this.toastService.error('Failed to load products list.');
        this.loading.set(false);
      }
    });
  }

  setPage(pageNum: number) {
    if (pageNum >= 1 && pageNum <= this.totalPages()) {
      this.page.set(pageNum);
      this.loadProducts();
    }
  }

  addProduct() {
    this.router.navigate(['/vendor-home/add-product']);
  }

  openEditProductModal(product: ProductResponse) {
    this.editingProduct.set(product);
    this.editProductName = product.name;
    this.editProductDescription = product.description || '';
    this.editProductCategoryId = product.categoryId;
    this.showEditProductModal.set(true);
  }

  closeEditProductModal() {
    this.showEditProductModal.set(false);
    this.editingProduct.set(null);
  }

  saveProductDetails(event: Event) {
    event.preventDefault();
    const product = this.editingProduct();
    if (!product) return;

    this.productService.updateProduct(product.id, {
      name: this.editProductName,
      description: this.editProductDescription,
      categoryId: this.editProductCategoryId || undefined
    }).subscribe({
      next: () => {
        this.toastService.success('Product details updated successfully.');
        this.closeEditProductModal();
        this.loadProducts();
      },
      error: (err) => {
        console.error('Error updating product details:', err);
        this.toastService.error('Failed to update product details.');
      }
    });
  }

  toggleProductStatus(product: ProductResponse) {
    const isCurrentlyArchived = product.status === 'Archived';
    const actionText = isCurrentlyArchived ? 'activate' : 'deactivate';

    if (confirm(`Are you sure you want to ${actionText} this product listing?`)) {
      const request$ = this.productService.toggleProductStatus(product.id);

      request$.subscribe({
        next: () => {
          this.toastService.success(`Product has been successfully ${isCurrentlyArchived ? 'activated' : 'deactivated'}.`);
          this.loadProducts();
        },
        error: (err: any) => {
          console.error(`Error trying to ${actionText} product`, err);
          this.toastService.error(`Failed to ${actionText} product listing.`);
        }
      });
    }
  }

  publishProduct(product: ProductResponse) {
    this.productService.publishProduct(product.id).subscribe({
      next: () => {
        this.toastService.success('Product published successfully.');
        this.loadProducts();
      },
      error: (err) => {
        console.error('Error publishing product:', err);
        this.toastService.error(err.error?.message || 'Failed to publish product.');
      }
    });
  }
}
