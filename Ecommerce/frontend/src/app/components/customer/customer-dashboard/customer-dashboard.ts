import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { ProductResponse, PageResponse } from '../../../models/product.model';
import { ProductCard } from '../../shared/product-card/product-card';
import { HeroCarouselComponent } from './components/hero-carousel/hero-carousel';
import { CategoryListComponent } from './components/category-list/category-list';
import { PromoBannersComponent } from './components/promo-banners/promo-banners';
import { TrustBadgeComponent } from './components/trust-badge/trust-badge';
import { ActivityPromosComponent } from './components/activity-promos/activity-promos';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ProductCard,
    HeroCarouselComponent,
    CategoryListComponent,
    PromoBannersComponent,
    TrustBadgeComponent,
    ActivityPromosComponent,
  ],
  templateUrl: './customer-dashboard.html',
  styleUrl: './customer-dashboard.css',
})
export class CustomerDashboard implements OnInit {
  newArrivals = signal<ProductResponse[]>([]);
  loading = signal<boolean>(true);

  constructor(private productService: ProductService) {}

  ngOnInit() {
    this.loadNewArrivals();
  }

  loadNewArrivals() {
    this.loading.set(true);
    this.productService.getCatalog({
      sortBy: 'newest',
      sortOrder: 'desc',
      pageSize: 6,
      pageNumber: 1
    }).subscribe({
      next: (res: PageResponse<ProductResponse>) => {
        this.newArrivals.set(res.items || []);
        this.loading.set(false);
      },
      error: (err: any) => {
        console.error('Error fetching new arrivals catalog:', err);
        this.loading.set(false);
      }
    });
  }
}
