import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-promo-banners',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './promo-banners.html',
  styleUrl: './promo-banners.css',
})
export class PromoBannersComponent {
  constructor(private router: Router) {}

  onCtaClick(sortBy: string, sortOrder?: string) {
    const queryParams: any = { sortBy };
    if (sortOrder) {
      queryParams.sortOrder = sortOrder;
    }
    this.router.navigate(['/customer-home/products-list'], { queryParams });
  }
}
