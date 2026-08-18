import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { Category } from '../../../models/category.model';
import { ProductVariantResponse } from '../../../models/product.model';
import { AddVariant } from '../add-variant/add-variant';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, AddVariant],
  templateUrl: './add-product.html',
  styleUrl: './add-product.css'
})
export class AddProduct implements OnInit {
  categories = signal<Category[]>([]);
  createdProductId = signal<number | null>(null);
  variantsAdded = signal<ProductVariantResponse[]>([]);
  showAddVariantForm = signal<boolean>(false);
  
  productName = signal<string>('');
  description = signal<string>('');
  categoryId = signal<number | null>(null);

  savingProduct = signal<boolean>(false);
  publishing = signal<boolean>(false);
  errorMsg = signal<string>('');
  successMsg = signal<string>('');

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats || []);
        if (cats && cats.length > 0) {
          this.categoryId.set(cats[0].id);
        }
      },
      error: (err) => {
        console.error('Error loading categories', err);
        this.errorMsg.set('Failed to load product categories.');
      }
    });
  }

  saveProduct() {
    this.errorMsg.set('');
    this.successMsg.set('');

    if (!this.productName().trim()) {
      this.errorMsg.set('Product name is required.');
      return;
    }

    if (!this.categoryId()) {
      this.errorMsg.set('Please select a product category.');
      return;
    }

    this.savingProduct.set(true);

    const request = {
      name: this.productName().trim(),
      description: this.description().trim(),
      categoryId: this.categoryId()!
    };

    this.productService.createProduct(request).subscribe({
      next: (res) => {
        this.savingProduct.set(false);
        this.createdProductId.set(res.data.id);
        this.successMsg.set('Product details saved successfully. You can now add variants.');
      },
      error: (err) => {
        console.error('Error creating product', err);
        this.errorMsg.set(err.error?.message || 'Failed to create product. Please try again.');
        this.savingProduct.set(false);
      }
    });
  }

  openAddVariant() {
    this.showAddVariantForm.set(true);
  }

  onVariantSaved(variant: ProductVariantResponse) {
    this.variantsAdded.update(prev => [...prev, variant]);
    this.showAddVariantForm.set(false);
    this.successMsg.set(`Variant #${this.variantsAdded().length} added successfully!`);
  }

  onVariantDiscarded() {
    this.showAddVariantForm.set(false);
  }

  publishProduct() {
    if (!this.createdProductId()) return;
    if (this.variantsAdded().length === 0) {
      this.errorMsg.set('You must add at least one variant before publishing.');
      return;
    }

    this.publishing.set(true);
    this.errorMsg.set('');

    this.productService.publishProduct(this.createdProductId()!).subscribe({
      next: () => {
        this.publishing.set(false);
        this.router.navigate(['/vendor-home/products-list']);
      },
      error: (err) => {
        console.error('Error publishing product', err);
        this.errorMsg.set(err.error?.message || 'Failed to publish product. Please try again.');
        this.publishing.set(false);
      }
    });
  }

  goBack() {
    this.router.navigate(['/vendor-home/products-list']);
  }
}
