import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CategoryService } from '../../../../../services/category.service';
import { Category } from '../../../../../models/category.model';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category-list.html',
  styleUrl: './category-list.css',
})
export class CategoryListComponent implements OnInit {
  categories = signal<Category[]>([]);
  loading = signal<boolean>(true);

  constructor(
    private categoryService: CategoryService,
    private router: Router
  ) {}

  ngOnInit() {
    this.categoryService.getCategories().subscribe({
      next: (cats) => {
        this.categories.set(cats || []);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching categories', err);
        this.loading.set(false);
      }
    });
  }

  getCategoryMeta(name: string): { icon: string; bgClass: string; color: string } {
    const n = name.toLowerCase();
    if (n.includes('elect')) {
      return { icon: 'bi-lightning-charge', bgClass: 'bg-electronics', color: '#3b82f6' };
    } else if (n.includes('laptop')) {
      return { icon: 'bi-laptop', bgClass: 'bg-laptop', color: '#0284c7' };
    } else if (n.includes('mobile') || n.includes('phone')) {
      return { icon: 'bi-phone', bgClass: 'bg-mobile', color: '#0ea5e9' };
    } else if (n.includes('tv') || n.includes('television')) {
      return { icon: 'bi-tv', bgClass: 'bg-tv', color: '#8b5cf6' };
    } else if (n.includes('audio') || n.includes('headphone') || n.includes('sound')) {
      return { icon: 'bi-headphones', bgClass: 'bg-audio', color: '#ec4899' };
    } else if (n.includes('fash') || n.includes('cloth') || n.includes('wear')) {
      return { icon: 'bi-handbag', bgClass: 'bg-fashion', color: '#8b5cf6' };
    } else if (n.includes('home') || n.includes('liv') || n.includes('furn')) {
      return { icon: 'bi-house', bgClass: 'bg-home', color: '#f97316' };
    } else if (n.includes('book')) {
      return { icon: 'bi-book', bgClass: 'bg-books', color: '#10b981' };
    } else if (n.includes('sport') || n.includes('fit')) {
      return { icon: 'bi-award', bgClass: 'bg-sports', color: '#ef4444' };
    } else if (n.includes('beaut') || n.includes('cosm')) {
      return { icon: 'bi-sparkles', bgClass: 'bg-beauty', color: '#ec4899' };
    } else if (n.includes('groc') || n.includes('food')) {
      return { icon: 'bi-basket', bgClass: 'bg-grocery', color: '#059669' };
    } else if (n.includes('toy') || n.includes('game')) {
      return { icon: 'bi-gift', bgClass: 'bg-toys', color: '#0ea5e9' };
    }
    return { icon: 'bi-tag', bgClass: 'bg-default', color: '#6b7280' };
  }

  selectCategory(categoryId: number) {
    this.router.navigate(['/customer-home/products-list'], {
      queryParams: { categoryId: categoryId }
    });
  }

  viewAllProducts() {
    this.router.navigate(['/customer-home/products-list']);
  }
}
