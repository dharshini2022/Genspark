import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { BusSearchResult } from '../../models/models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  searchForm: FormGroup;
  buses: BusSearchResult[] = [];
  loading = false;
  searched = false;
  today = new Date().toISOString().split('T')[0];

  constructor(private fb: FormBuilder, private userSvc: UserService, private router: Router) {
    const filters = this.userSvc.lastFilters;
    this.searchForm = this.fb.group({
      source:      [filters?.source || '', Validators.required],
      destination: [filters?.destination || '', Validators.required],
      date:        [filters?.date || this.today, Validators.required]
    });
    if (this.userSvc.lastSearchResults.length > 0) {
      this.buses = this.userSvc.lastSearchResults;
      this.searched = true;
    }
  }

  search(): void {
    if (this.searchForm.invalid) { this.searchForm.markAllAsTouched(); return; }
    this.loading = true; this.searched = true;
    const { source, destination, date } = this.searchForm.value;
    this.userSvc.searchBuses(source, destination, date).subscribe({
      next: buses => { 
        this.buses = buses; 
        this.userSvc.lastSearchResults = buses;
        this.loading = false; 
      },
      error: ()   => { this.loading = false; }
    });
  }

  selectBus(bus: BusSearchResult): void {
    this.router.navigate(['/user/bus-details'], { state: { bus } });
  }

  formatTime(dt: string): string {
    return new Date(dt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true });
  }

  getDuration(dep: string, arr: string): string {
    const d = new Date(dep);
    const a = new Date(arr);
    const diffMs = a.getTime() - d.getTime();
    const hrs = Math.floor(diffMs / 3600000);
    const mins = Math.round((diffMs % 3600000) / 60000);
    return `${hrs}h ${mins}m`;
  }
}
