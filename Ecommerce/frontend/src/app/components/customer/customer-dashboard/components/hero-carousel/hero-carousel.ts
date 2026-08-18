import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

interface Slide {
  id: number;
  label: string;
  title: string;
  subtitle: string;
  btnText: string;
  bgImage: string;
  queryParams: any;
}

@Component({
  selector: 'app-hero-carousel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hero-carousel.html',
  styleUrl: './hero-carousel.css',
})
export class HeroCarouselComponent implements OnInit, OnDestroy {
  activeIdx = signal<number>(0);
  private autoPlayInterval: any;

  slides: Slide[] = [
    {
      id: 1,
      label: 'HOT DEALS',
      title: 'Top Rated Picks',
      subtitle: 'Thousands of 5-star reviewed products, curated for you.',
      btnText: 'Browse Picks →',
      bgImage: 'https://images.unsplash.com/photo-1498049794561-7780e7231661?q=80&w=1600&auto=format&fit=crop',
      queryParams: { sortBy: 'rating', sortOrder: 'desc' }
    },
    {
      id: 2,
      label: 'SEASON SALE',
      title: 'Limited Time Offers',
      subtitle: 'Up to 50% off on premium apparel, electronics, and home essentials.',
      btnText: 'See All Deals →',
      bgImage: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?q=80&w=1600&auto=format&fit=crop',
      queryParams: { sortBy: 'discount' }
    },
    {
      id: 3,
      label: 'JUST IN',
      title: 'Fresh New Arrivals',
      subtitle: 'Explore the latest releases and cutting edge gear added today.',
      btnText: 'Shop New →',
      bgImage: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?q=80&w=1600&auto=format&fit=crop',
      queryParams: { sortBy: 'newest', sortOrder: 'desc' }
    }
  ];

  constructor(private router: Router) {}

  ngOnInit() {
    this.startAutoPlay();
  }

  ngOnDestroy() {
    this.stopAutoPlay();
  }

  startAutoPlay() {
    this.stopAutoPlay();
    this.autoPlayInterval = setInterval(() => {
      this.activeIdx.update(current => (current + 1) % this.slides.length);
    }, 5000);
  }

  stopAutoPlay() {
    if (this.autoPlayInterval) {
      clearInterval(this.autoPlayInterval);
    }
  }

  setSlide(index: number) {
    this.activeIdx.set(index);
    this.startAutoPlay(); 
  }

  onActionClick(queryParams: any) {
    this.router.navigate(['/customer-home/products-list'], {
      queryParams: queryParams
    });
  }
}
