import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../services/admin.service';
import { RevenueDto } from '../../models/models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  revenue: RevenueDto | null = null;
  loading = true;

  constructor(private svc: AdminService) {}

  ngOnInit(): void {
    this.svc.getRevenue().subscribe({
      next: r => { this.revenue = r; this.loading = false; },
      error: () => this.loading = false
    });
  }
}
