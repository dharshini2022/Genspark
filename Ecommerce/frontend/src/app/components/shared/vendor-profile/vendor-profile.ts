import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { VendorService } from '../../../services/vendor.service';
import { ProductService } from '../../../services/product.service';
import { DiscountService } from '../../../services/disocunt.service';
import { VendorProfileResponse, VendorStatus } from '../../../models/vendor.model';
import { ProductResponse } from '../../../models/product.model';
import { DiscountResponse, CreateDiscountRequest } from '../../../models/disocunt.model';
import { forkJoin, catchError, of } from 'rxjs';
import { ToastService } from '../../../services/toast.service';
import { ProductList } from '../../vendor/product-list/product-list';
import { SettlementList } from '../settlement-list/settlement-list';
import { DiscountList } from '../discount-list/discount-list';

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-vendor-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ProductList, SettlementList, DiscountList, ResolveImagePipe],
  templateUrl: './vendor-profile.html',
  styleUrl: './vendor-profile.css'
})
export class VendorProfile implements OnInit {
  isAdminView = signal<boolean>(false);
  vendorProfile = signal<VendorProfileResponse | null>(null);
  products = signal<ProductResponse[]>([]);
  discounts = signal<DiscountResponse[]>([]);
  settlements = signal<any[]>([]);

  activeTab = signal<string>('products');
  settlementsLoaded = signal<boolean>(false);
  discountsLoaded = signal<boolean>(false);

  revenueGenerated = signal<number>(0);
  loading = signal<boolean>(true);
  errorMsg = signal<string>('');
  successMsg = signal<string>('');

  revealGST = signal<boolean>(false);
  revealPAN = signal<boolean>(false);
  showPassword = signal<boolean>(false);

  VendorStatusEnum = VendorStatus;

  productsCount = computed(() => this.products().length);
  activeDiscountsCount = computed(() => {
    const list = this.discounts();
    return list.filter(d => d.isActive).length.toString();
  });
  settlementsCount = computed(() => {
    const list = this.settlements();
    return list.length.toString();
  });

  isPending = computed(() => {
    const profile = this.vendorProfile();
    if (!profile) return false;
    const status = profile.status as any;
    return status === VendorStatus.Pending || 
           status === 'Pending' || 
           status === 1;
  });

  isApproved = computed(() => {
    const profile = this.vendorProfile();
    if (!profile) return false;
    const status = profile.status as any;
    return status === VendorStatus.Approved || 
           status === 'Approved' || 
           status === 2;
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private productService: ProductService,
    private discountService: DiscountService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        this.isAdminView.set(true);
        const id = Number(idStr);
        this.loadAdminData(id);
      } else {
        this.isAdminView.set(false);
        this.loadVendorData();
      }
    });
  }

  loadAdminData(id: number) {
    this.loading.set(true);
    this.errorMsg.set('');

    forkJoin({
      profile: this.vendorService.getVendorProfileById(id),
      adminRevenue: this.vendorService.getAdminRevenueForVendor(id).pipe(catchError(() => of({ revenue: 0 }))),
      products: this.productService.getProductsByVendorId(id, 1, 1000).pipe(catchError(() => of({ items: [] }))),
      discounts: this.discountService.getVendorDiscountsByAdmin(id, 1, 1000).pipe(catchError(() => of({ items: [] })))
    }).subscribe({
      next: (res) => {
        this.vendorProfile.set(res.profile);
        this.revenueGenerated.set(res.adminRevenue?.revenue || 0);
        this.products.set(res.products?.items || []);
        this.discounts.set(res.discounts?.items || []);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading admin profile data', err);
        this.errorMsg.set('Failed to load merchant profile.');
        this.loading.set(false);
      }
    });
  }

  loadVendorData() {
    this.loading.set(true);
    this.errorMsg.set('');

    forkJoin({
      profile: this.vendorService.getMyVendorProfile(),
      settlements: this.vendorService.getMySettlements(1, 1000).pipe(catchError(() => of({ items: [] }))),
      products: this.productService.getVendorProducts(1, 1000).pipe(catchError(() => of({ items: [] }))),
      discounts: this.discountService.getMyVendorDiscounts(1, 1000).pipe(catchError(() => of({ items: [] })))
    }).subscribe({
      next: (res) => {
        this.vendorProfile.set(res.profile);
        const sum = (res.settlements.items || []).reduce((acc, curr) => acc + (curr.netPayoutAmount || curr.grossAmount || 0), 0);
        this.revenueGenerated.set(sum);
        this.settlements.set(res.settlements.items || []);
        this.products.set(res.products?.items || []);
        this.discounts.set(res.discounts?.items || []);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading vendor self data', err);
        this.errorMsg.set('Failed to load your vendor profile details.');
        this.loading.set(false);
      }
    });
  }

  setTab(tab: string) {
    this.activeTab.set(tab);
    if (tab === 'settlements') {
      this.settlementsLoaded.set(true);
    } else if (tab === 'discounts') {
      this.discountsLoaded.set(true);
    }
  }

  toggleVendorStatus() {
    const profile = this.vendorProfile();
    if (!profile) return;

    this.loading.set(true);
    this.successMsg.set('');
    this.errorMsg.set('');

    const targetId = this.isAdminView() ? profile.id : undefined;
    this.vendorService.toggleVendorStatus(targetId).subscribe({
      next: () => {
        const actionStr = profile.status !== VendorStatus.Approved ? 'activated' : 'deactivated';
        
        if (this.isAdminView()) {
          this.loadAdminData(profile.id);
        } else {
          this.loadVendorData();
        }

        this.toastService.success(`Account has been successfully ${actionStr}!`);
      },
      error: (err) => {
        console.error('Error toggling vendor status', err);
        this.toastService.error('Failed to toggle store status.');
        this.loading.set(false);
      }
    });
  }

  approveVendor() {
    const profile = this.vendorProfile();
    if (!profile) return;

    this.loading.set(true);
    this.successMsg.set('');
    this.errorMsg.set('');

    this.vendorService.approveVendor(profile.id).subscribe({
      next: () => {
        if (this.isAdminView()) {
          this.loadAdminData(profile.id);
        } else {
          this.loadVendorData();
        }
        this.toastService.success('Vendor has been successfully approved!');
      },
      error: (err) => {
        console.error('Error approving vendor', err);
        this.toastService.error('Failed to approve vendor.');
        this.loading.set(false);
      }
    });
  }

  getInitials(storeName: string): string {
    if (!storeName) return 'V';
    const parts = storeName.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return storeName.substring(0, 2).toUpperCase();
  }

  getAppliedDate(vendorId: number): string {
    const day = (12 + (vendorId * 3) % 17).toString().padStart(2, '0');
    return `Jun ${day}, 2026`;
  }

  getCategoryForMock(vendorId: number): string {
    const categories = ['Home & Living', 'Beauty', 'Electronics', 'Grocery', 'Fashion', 'Sports'];
    return categories[vendorId % categories.length];
  }
}
