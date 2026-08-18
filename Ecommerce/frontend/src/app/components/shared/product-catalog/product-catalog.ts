import { Component, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ProductResponse, ProductWithDiscount } from '../../../models/product.model';
import { Category } from '../../../models/category.model';
import { DiscountResponse, DiscountType } from '../../../models/disocunt.model';
import { ProductSidebar } from '../product-sidebar/product-sidebar';
import { ProductSort } from '../product-sort/product-sort';
import { ProductsList } from '../products-list/products-list';
import { forkJoin, of, Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-product-catalog',
  standalone: true,
  imports: [CommonModule, ProductSidebar, ProductSort, ProductsList],
  templateUrl: './product-catalog.html',
  styleUrl: './product-catalog.css'
})
export class ProductCatalog implements OnInit {
  products = signal<ProductWithDiscount[]>([]);
  categories = signal<Category[]>([]);
  loading = signal<boolean>(true);
  loadingMore = signal<boolean>(false);

  selectedCategoryId = signal<number | undefined>(this.getInitialCategoryId());
  minPrice = signal<number>(this.getInitialMinPrice());
  maxPrice = signal<number>(this.getInitialMaxPrice());
  currentSort = signal<string>(this.getInitialSort());
  totalProducts = signal<number>(0);

  draftSort = signal<string>(this.getInitialSort());
  isPriceFilterActive = signal<boolean>(this.getInitialPriceFilterActive());
  isSidebarOpen = signal<boolean>(false);

  currentPage = 1;
  pageSize = 12;
  hasNext = true;
  searchQuery = signal<string | undefined>(this.getInitialSearchQuery());

  backendSortBy = computed(() => {
    const s = this.currentSort();
    if (s === 'price_asc' || s === 'price_desc') return 'price';
    if (s === 'rating') return 'rating';
    if (s === 'discount') return 'discount';
    return 'newest';
  });

  backendSortOrder = computed(() => {
    const s = this.currentSort();
    if (s === 'price_asc') return 'asc';
    return 'desc';
  });

  private getInitialCategoryId(): number | undefined {
    const cached = sessionStorage.getItem('catalog_categoryId');
    return cached ? Number(cached) : undefined;
  }

  private getInitialMinPrice(): number {
    const cached = sessionStorage.getItem('catalog_minPrice');
    return cached ? Number(cached) : 0;
  }

  private getInitialMaxPrice(): number {
    const cached = sessionStorage.getItem('catalog_maxPrice');
    return cached ? Number(cached) : 1000000;
  }

  private getInitialSort(): string {
    const cached = sessionStorage.getItem('catalog_currentSort');
    return cached ? cached : 'newest';
  }

  private getInitialSearchQuery(): string | undefined {
    return undefined;
  }

  private getInitialPriceFilterActive(): boolean {
    const cached = sessionStorage.getItem('catalog_isPriceFilterActive');
    return cached === 'true';
  }

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private categoryService: CategoryService,
    private discountService: DiscountService
  ) {
    effect(() => {
      const categoryId = this.selectedCategoryId();
      if (categoryId !== undefined) {
        sessionStorage.setItem('catalog_categoryId', categoryId.toString());
      } else {
        sessionStorage.removeItem('catalog_categoryId');
      }
    });

    effect(() => {
      sessionStorage.setItem('catalog_minPrice', this.minPrice().toString());
    });

    effect(() => {
      sessionStorage.setItem('catalog_maxPrice', this.maxPrice().toString());
    });

    effect(() => {
      sessionStorage.setItem('catalog_isPriceFilterActive', this.isPriceFilterActive().toString());
    });

    effect(() => {
      sessionStorage.setItem('catalog_currentSort', this.currentSort());
    });
  }

  ngOnInit() {
    this.categoryService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats || []),
      error: (err) => console.error('Error loading categories:', err)
    });

    let isInitialized = false;
    this.route.queryParams.subscribe(params => {

      if (Object.keys(params).length > 0) {
        if (params['categoryId'] !== undefined) {
          const catIdStr = params['categoryId'];
          if (catIdStr) {
            this.selectedCategoryId.set(Number(catIdStr));
          } else {
            this.selectedCategoryId.set(undefined);
          }
        }

        if (params['search'] !== undefined) {
          const searchParam = params['search'];
          if (searchParam) {
            this.searchQuery.set(searchParam);
          } else {
            this.searchQuery.set(undefined);
          }
        } else {
          this.searchQuery.set(undefined);
        }

        const minP = params['minPrice'];
        const maxP = params['maxPrice'];
        if (minP !== undefined && maxP !== undefined) {
          this.minPrice.set(Number(minP));
          this.maxPrice.set(Number(maxP));
          this.isPriceFilterActive.set(true);
        }

        if (params['sortBy'] !== undefined) {
          const sortByParam = params['sortBy'];
          const sortOrderParam = params['sortOrder'];
          let sortStr = 'newest';
          if (sortByParam === 'discount') {
            sortStr = 'discount';
          } else if (sortByParam === 'price') {
            sortStr = `price_${sortOrderParam || 'asc'}`;
          } else {
            sortStr = sortByParam;
          }
          this.currentSort.set(sortStr);
          this.draftSort.set(sortStr);
        }
      } else {
        this.searchQuery.set(undefined);
        this.draftSort.set(this.currentSort());
      }

      if (isInitialized) {
        this.loadProducts(true);
      }
    });

    this.loadProducts(true);
    isInitialized = true;
  }

  loadProducts(isFirstPage: boolean = false) {
    if (isFirstPage) {
      this.currentPage = 1;
      this.loading.set(true);
      this.hasNext = true;
    } else {
      this.loadingMore.set(true);
    }

    const query = this.searchQuery();
    const filter = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      categoryId: this.selectedCategoryId(),
      sortBy: this.backendSortBy(),
      sortOrder: this.backendSortOrder(),
      searchQuery: query && query.trim() !== '' ? query : undefined,
      minPrice: this.isPriceFilterActive() ? this.minPrice() : undefined,
      maxPrice: this.isPriceFilterActive() ? this.maxPrice() : undefined
    };

    this.productService.getCatalog(filter).subscribe({
      next: (res) => {
        this.enrichProductsWithDiscounts(res.items || []).subscribe(enriched => {
          if (isFirstPage) {
            this.products.set(enriched);
          } else {
            this.products.update(items => [...items, ...enriched]);
          }
          this.totalProducts.set(res.totalCount || 0);
          this.hasNext = res.hasNext;
          this.loading.set(false);
          this.loadingMore.set(false);
        });
      },
      error: (err) => {
        console.error('Error loading products:', err);
        if (isFirstPage) {
          this.products.set([]);
          this.totalProducts.set(0);
        }
        this.hasNext = false;
        this.loading.set(false);
        this.loadingMore.set(false);
      }
    });
  }

  onLoadNextPage() {
    this.currentPage++;
    this.loadProducts(false);
  }

  onCategorySelectedFromEmptyState(categoryId: number) {
    this.selectedCategoryId.set(categoryId);
    this.searchQuery.set(undefined);
    this.loadProducts(true);
  }

  onApplyFilters(event: { categoryId: number | undefined, minPrice: number, maxPrice: number }) {
    this.selectedCategoryId.set(event.categoryId);
    this.minPrice.set(event.minPrice);
    this.maxPrice.set(event.maxPrice);
    this.isPriceFilterActive.set(true);
    this.loadProducts(true);
    this.isSidebarOpen.set(false);
  }

  onSortChanged(event: { sortBy: string, sortOrder: string }) {
    let sortStr = 'newest';
    if (event.sortBy === 'discount') {
      sortStr = 'discount';
    } else if (event.sortBy === 'price') {
      sortStr = `price_${event.sortOrder}`;
    } else {
      sortStr = event.sortBy;
    }
    this.currentSort.set(sortStr);
    this.draftSort.set(sortStr);
    this.loadProducts(true);
  }

  onClearFilters() {
    this.selectedCategoryId.set(undefined);
    this.minPrice.set(0);
    this.maxPrice.set(1000000);
    this.currentSort.set('newest');
    this.draftSort.set('newest');
    this.isPriceFilterActive.set(false);
    this.searchQuery.set(undefined);
    this.loadProducts(true);
    this.isSidebarOpen.set(false);
  }

  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }

  enrichProductsWithDiscounts(prods: ProductResponse[]): Observable<ProductWithDiscount[]> {
    if (!prods || prods.length === 0) return of([]);

    const obs = prods.map(prod => {
      const variant = prod.variants?.find(v => v.isDefault) || prod.variants?.find(v => v.isActive);
      if (!variant) {
        return of({ ...prod, discountLabel: '', discountedPrice: 0, discountPercent: 0 } as ProductWithDiscount);
      }

      return this.discountService.getDiscountsOfProduct(prod.id, prod.categoryId, prod.vendorId).pipe(
        map(discounts => {
          let label = '';
          let discountedPrice = variant.price;
          let discountPercent = 0;

          if (discounts && discounts.length > 0) {
            let bestDiscount = discounts[0];
            let maxDiscountAmount = 0;

            discounts.forEach((d: DiscountResponse) => {
              if (d.minOrderValue && variant.price < d.minOrderValue) {
                return;
              }
              let amount = 0;
              if (d.type === DiscountType.Flat) {
                amount = d.value;
              } else if (d.type === DiscountType.Percentage) {
                amount = (d.value / 100) * variant.price;
              }

              if (amount > maxDiscountAmount) {
                maxDiscountAmount = amount;
                bestDiscount = d;
              }
            });

            if (maxDiscountAmount > 0) {
              if (bestDiscount.type === DiscountType.Flat) {
                label = `Rs. ${bestDiscount.value}`;
                discountedPrice = Math.max(0, variant.price - bestDiscount.value);
                discountPercent = (bestDiscount.value / variant.price) * 100;
              } else if (bestDiscount.type === DiscountType.Percentage) {
                label = `${bestDiscount.value}%`;
                discountedPrice = parseFloat((variant.price * (1 - bestDiscount.value / 100)).toFixed(2));
                discountPercent = bestDiscount.value;
              }
            }
          }

          return {
            ...prod,
            discountLabel: label,
            discountedPrice,
            discountPercent
          } as ProductWithDiscount;
        }),
        catchError(() => {
          return of({
            ...prod,
            discountLabel: '',
            discountedPrice: variant.price,
            discountPercent: 0
          } as ProductWithDiscount);
        })
      );
    });

    return forkJoin(obs);
  }
}
