import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { VendorService } from '../../../services/vendor.service';
import { VendorProfileResponse, VendorStatus } from '../../../models/vendor.model';
import { forkJoin } from 'rxjs';

import { ResolveImagePipe } from '../../../pipes/resolve-image.pipe';

@Component({
  selector: 'app-vendor-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ResolveImagePipe],
  templateUrl: './vendor-list.html',
  styleUrl: './vendor-list.css'
})
export class VendorList implements OnInit {
  vendors = signal<VendorProfileResponse[]>([]);
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');
  activeTab = signal<'pending' | 'all'>('pending');
  selectedReviewVendor = signal<VendorProfileResponse | null>(null);
  
  errorMsg = signal<string>('');
  successMsg = signal<string>('');
  processingAction = signal<boolean>(false);

  VendorStatusEnum = VendorStatus;

  pendingCount = signal<number>(0);
  activeCount = signal<number>(0);
  suspendedCount = signal<number>(0);
  totalCount = signal<number>(0);

  filteredVendors = computed(() => {
    let list = this.vendors();

    const query = this.searchTerm().trim().toLowerCase();
    if (query) {
      list = list.filter(v => 
        v.storeName.toLowerCase().includes(query) ||
        v.storeEmail.toLowerCase().includes(query) ||
        v.userFullName.toLowerCase().includes(query) ||
        v.userEmail.toLowerCase().includes(query)
      );
    }

    return list;
  });

  constructor(private vendorService: VendorService) {}

  ngOnInit() {
    this.loadCounts();
    this.loadVendors();
  }

  loadCounts() {
    forkJoin({
      pending: this.vendorService.getVendorsByStatus('Pending'),
      active: this.vendorService.getVendorsByStatus('Approved'),
      suspended: this.vendorService.getVendorsByStatus('Cancelled')
    }).subscribe({
      next: (res) => {
        this.pendingCount.set(res.pending.length);
        this.activeCount.set(res.active.length);
        this.suspendedCount.set(res.suspended.length);
        this.totalCount.set(res.pending.length + res.active.length + res.suspended.length);
      },
      error: (err) => console.error('Error fetching vendor counts', err)
    });
  }

  loadVendors() {
    this.loading.set(true);
    this.errorMsg.set('');

    if (this.activeTab() === 'pending') {
      this.vendorService.getVendorsByStatus('Pending').subscribe({
        next: (list) => {
          this.vendors.set(list || []);
          this.loading.set(false);
          this.pendingCount.set(list.length);
        },
        error: (err) => {
          console.error('Error fetching pending vendors', err);
          this.errorMsg.set('Failed to load pending vendors list.');
          this.loading.set(false);
        }
      });
    } else {
      this.vendorService.getAllVendors(1, 200).subscribe({
        next: (res) => {
          const list = res.items || [];
          this.vendors.set(list);
          this.loading.set(false);
          this.totalCount.set(res.totalCount || list.length);
        },
        error: (err) => {
          console.error('Error fetching all vendors', err);
          this.errorMsg.set('Failed to load vendors list.');
          this.loading.set(false);
        }
      });
    }
  }

  setActiveTab(tab: 'pending' | 'all') {
    this.activeTab.set(tab);
    this.loadVendors();
  }

  openReview(vendor: VendorProfileResponse) {
    this.selectedReviewVendor.set(vendor);
  }

  closeReview() {
    this.selectedReviewVendor.set(null);
  }

  approveVendor(id: number) {
    this.processingAction.set(true);
    this.errorMsg.set('');
    this.successMsg.set('');
    this.vendorService.approveVendor(id).subscribe({
      next: (res) => {
        this.successMsg.set(`Store "${res.storeName || 'Vendor'}" has been approved successfully.`);
        this.closeReview();
        this.processingAction.set(false);
        this.loadCounts();
        this.loadVendors();
        // Clear message after 3s
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        console.error('Error approving vendor', err);
        this.errorMsg.set('Failed to approve vendor application.');
        this.processingAction.set(false);
      }
    });
  }

  rejectVendor(id: number) {
    this.processingAction.set(true);
    this.errorMsg.set('');
    this.successMsg.set('');
    this.vendorService.cancelVendor(id).subscribe({
      next: (res) => {
        this.successMsg.set(`Store "${res.storeName || 'Vendor'}" has been rejected.`);
        this.closeReview();
        this.processingAction.set(false);
        this.loadCounts();
        this.loadVendors();
        // Clear message after 3s
        setTimeout(() => this.successMsg.set(''), 3000);
      },
      error: (err) => {
        console.error('Error rejecting vendor', err);
        this.errorMsg.set('Failed to reject vendor application.');
        this.processingAction.set(false);
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
