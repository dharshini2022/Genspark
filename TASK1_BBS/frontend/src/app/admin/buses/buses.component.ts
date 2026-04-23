import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { BusPendingDto } from '../../models/models';

@Component({
  selector: 'app-admin-buses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './buses.component.html',
  styleUrl: './buses.component.scss'
})
export class AdminBusesComponent implements OnInit {
  buses: BusPendingDto[] = [];
  loading = true;
  actionId: number | null = null;
  filters = ['All', 'Pending', 'Active', 'Down'];
  activeFilter = 'Pending';

  constructor(private svc: AdminService, private toast: ToastService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    const status = this.activeFilter === 'All' ? undefined : this.activeFilter;
    this.svc.getBuses(status).subscribe({
      next: b => { this.buses = b; this.loading = false; },
      error: () => this.loading = false
    });
  }

  setFilter(f: string): void { this.activeFilter = f; this.loading = true; this.load(); }

  approve(id: number): void {
    this.actionId = id;
    this.svc.approveBus(id).subscribe({
      next: r => { this.toast.success('Bus approved!', r.message); this.actionId = null; this.load(); },
      error: err => { this.toast.error('Error', err.error?.message); this.actionId = null; }
    });
  }

  disable(id: number): void {
    this.actionId = id;
    this.svc.disableBus(id).subscribe({
      next: r => { this.toast.success('Bus disabled', r.message); this.actionId = null; this.load(); },
      error: err => { this.toast.error('Error', err.error?.message); this.actionId = null; }
    });
  }

  statusClass(s: string): string {
    return ({ Active: 'badge-success', Pending: 'badge-warning', Down: 'badge-danger', Removed: 'badge-muted' } as Record<string, string>)[s] ?? 'badge-muted';
  }
}
