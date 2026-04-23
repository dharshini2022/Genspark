import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';
import { OperatorProfile } from '../../models/models';

@Component({
  selector: 'app-admin-operators',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './operators.component.html',
  styleUrl: './operators.component.scss'
})
export class AdminOperatorsComponent implements OnInit {
  operators: OperatorProfile[] = [];
  loading = true;
  actionId: number | null = null;
  filters = ['All', 'Pending', 'Approved', 'Disabled'];
  activeFilter = 'All';

  constructor(private svc: AdminService, private toast: ToastService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    const status = this.activeFilter === 'All' ? undefined : this.activeFilter;
    this.svc.getOperators(status).subscribe({
      next: ops => { this.operators = ops; this.loading = false; },
      error: () => this.loading = false
    });
  }

  setFilter(f: string): void { this.activeFilter = f; this.loading = true; this.load(); }

  approve(id: number): void {
    this.actionId = id;
    this.svc.approveOperator(id).subscribe({
      next: r => { this.toast.success('Operator approved', r.message); this.actionId = null; this.load(); },
      error: err => { this.toast.error('Error', err.error?.message); this.actionId = null; }
    });
  }

  disable(id: number): void {
    this.actionId = id;
    this.svc.disableOperator(id).subscribe({
      next: r => { this.toast.success('Operator disabled', r.message); this.actionId = null; this.load(); },
      error: err => { this.toast.error('Error', err.error?.message); this.actionId = null; }
    });
  }

  statusClass(s: string): string {
    return ({ Approved: 'badge-success', Pending: 'badge-warning', Disabled: 'badge-danger' } as Record<string, string>)[s] ?? 'badge-muted';
  }
}
