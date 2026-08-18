import { Component, OnInit, signal, computed, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VendorService } from '../../../services/vendor.service';
import { ToastService } from '../../../services/toast.service';

@Component({
  selector: 'app-settlement-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settlement-list.html',
  styleUrl: './settlement-list.css'
})
export class SettlementList implements OnInit {
  @Input() vendorId: number | null = null;
  @Input() isAdminView: boolean = false;

  private vendorService = inject(VendorService);
  private toastService = inject(ToastService);

  settlements = signal<any[]>([]);
  loading = signal<boolean>(false);
  expandedSettlementId = signal<number | null>(null);

  page = signal<number>(1);
  pageSize = signal<number>(10);
  totalCount = signal<number>(0);
  totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  ngOnInit() {
    this.loadSettlements();
  }

  loadSettlements() {
    this.loading.set(true);
    const pNum = this.page();
    const pSize = this.pageSize();

    const request$ = this.isAdminView && this.vendorId
      ? this.vendorService.getVendorSettlementsById(this.vendorId, pNum, pSize)
      : this.vendorService.getMySettlements(pNum, pSize);

    request$.subscribe({
      next: (res) => {
        this.settlements.set(res.items || []);
        this.totalCount.set(res.totalCount || 0);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading settlements', err);
        this.toastService.error('Failed to load settlements list.');
        this.loading.set(false);
      }
    });
  }

  setPage(pageNum: number) {
    if (pageNum >= 1 && pageNum <= this.totalPages()) {
      this.page.set(pageNum);
      this.loadSettlements();
    }
  }

  toggleSettlementExpand(id: number) {
    if (this.expandedSettlementId() === id) {
      this.expandedSettlementId.set(null);
    } else {
      this.expandedSettlementId.set(id);
    }
  }

  getAppliedDate(id: number): string {
    const day = (12 + (id * 3) % 17).toString().padStart(2, '0');
    return `Jun ${day}, 2026`;
  }
}
