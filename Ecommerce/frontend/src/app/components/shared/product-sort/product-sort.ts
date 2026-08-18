import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-product-sort',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-sort.html',
  styleUrl: './product-sort.css'
})
export class ProductSort {
  totalProducts = input<number>(0);
  currentSort = input<string>('newest');

  sortChanged = output<{sortBy: string, sortOrder: string}>();
  toggleSidebar = output<void>();

  onSortClick(sortBy: string, sortOrder: string) {
    this.sortChanged.emit({ sortBy, sortOrder });
  }
}
