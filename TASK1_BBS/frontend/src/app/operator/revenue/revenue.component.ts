import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OperatorService } from '../../services/operator.service';
import { OperatorDetailedRevenueDto, BusDto } from '../../models/models';

@Component({
  selector: 'app-revenue',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './revenue.component.html',
  styleUrl: './revenue.component.scss'
})
export class RevenueComponent implements OnInit {
  revenueData: OperatorDetailedRevenueDto | null = null;
  buses: BusDto[] = [];
  loading = true;

  filterStartDate = '';
  filterEndDate = '';
  filterBusId: number | '' = '';

  constructor(private svc: OperatorService) {}

  ngOnInit(): void {
    this.svc.getBuses().subscribe(b => this.buses = b);
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    const sDate = this.filterStartDate ? new Date(this.filterStartDate).toISOString() : undefined;
    const eDate = this.filterEndDate ? new Date(this.filterEndDate).toISOString() : undefined;
    const bId = this.filterBusId !== '' ? Number(this.filterBusId) : undefined;

    this.svc.getDetailedRevenue(sDate, eDate, bId).subscribe({
      next: res => { this.revenueData = res; this.loading = false; },
      error: () => this.loading = false
    });
  }

  formatDate(dt: string): string {
    return new Date(dt).toLocaleString('en-IN', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  }

  clearFilters(): void {
    this.filterStartDate = '';
    this.filterEndDate = '';
    this.filterBusId = '';
    this.loadData();
  }
}
