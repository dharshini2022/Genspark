import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductReview } from './product-review';
import { ReviewResponse } from '../../../models/review.model';
import { describe, it, expect, beforeEach } from 'vitest';

const makeReview = (overrides: Partial<ReviewResponse> = {}): ReviewResponse => ({
  id: 1,
  productId: 10,
  productName: 'Test Product',
  userId: 100,
  userFullName: 'Alice',
  orderId: 5,
  rating: 4,
  title: 'Good product',
  body: 'Really liked it',
  createdAt: '2024-01-01T00:00:00',
  reviewImages: [],
  ...overrides
});

describe('ProductReview', () => {
  let component: ProductReview;
  let fixture: ComponentFixture<ProductReview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductReview]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductReview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('reviews Input', () => {
    it('should accept reviews via Input and expose them', () => {
      const reviews = [makeReview()];
      component.reviews = reviews;
      expect(component.reviews.length).toBe(1);
    });

    it('should default to empty array for null/undefined reviews', () => {
      component.reviews = null as any;
      expect(component.reviews).toEqual([]);
    });
  });

  describe('productAverageRating Input', () => {
    it('should accept average rating', () => {
      component.productAverageRating = 4.2;
      expect(component.productAverageRating).toBe(4.2);
    });

    it('should default to 0 for null/undefined average rating', () => {
      component.productAverageRating = null as any;
      expect(component.productAverageRating).toBe(0);
    });
  });

  describe('ratingDistribution', () => {
    it('should return zeros for all stars when reviews is empty', () => {
      component.reviews = [];
      const dist = component.ratingDistribution();
      expect(dist[5]).toBe(0);
      expect(dist[4]).toBe(0);
      expect(dist[3]).toBe(0);
      expect(dist[2]).toBe(0);
      expect(dist[1]).toBe(0);
    });

    it('should calculate distribution for given reviews', () => {
      component.reviews = [
        makeReview({ id: 1, rating: 5 }),
        makeReview({ id: 2, rating: 5 }),
        makeReview({ id: 3, rating: 3 }),
      ];
      const dist = component.ratingDistribution();
      expect(dist[5]).toBe(2);
      expect(dist[3]).toBe(1);
    });

    it('should ignore out-of-range ratings', () => {
      component.reviews = [
        makeReview({ id: 1, rating: 0 }),
        makeReview({ id: 2, rating: 6 }),
      ];
      const dist = component.ratingDistribution();
      const total = Object.values(dist).reduce((a, b) => a + b, 0);
      expect(total).toBe(0);
    });
  });

  describe('ratingPercentages', () => {
    it('should return zeros for empty reviews', () => {
      component.reviews = [];
      const pct = component.ratingPercentages();
      expect(pct[5]).toBe(0);
    });

    it('should calculate percentages based on reviews', () => {
      component.reviews = [
        makeReview({ id: 1, rating: 5 }),
        makeReview({ id: 2, rating: 5 }),
        makeReview({ id: 3, rating: 3 }),
        makeReview({ id: 4, rating: 3 }),
      ];
      const pct = component.ratingPercentages();
      expect(pct[5]).toBe(50);
      expect(pct[3]).toBe(50);
    });
  });

  describe('averageRatingValue', () => {
    it('should return productAverageRating if no reviews', () => {
      component.reviews = [];
      component.productAverageRating = 3.8;
      expect(component.averageRatingValue()).toBe(3.8);
    });

    it('should calculate average from reviews', () => {
      component.reviews = [
        makeReview({ id: 1, rating: 4 }),
        makeReview({ id: 2, rating: 2 })
      ];
      expect(component.averageRatingValue()).toBe(3);
    });
  });

  describe('getStarArray', () => {
    it('should return 5 full stars for rating 5', () => {
      const stars = component.getStarArray(5);
      expect(stars.length).toBe(5);
      expect(stars.every(s => s === 1)).toBe(true);
    });

    it('should include half star for rating with 0.5', () => {
      const stars = component.getStarArray(3.5);
      expect(stars).toContain(0.5);
    });

    it('should include empty stars for low rating', () => {
      const stars = component.getStarArray(1);
      expect(stars.filter(s => s === 0).length).toBe(4);
    });

    it('should return 5 elements always', () => {
      [0, 1, 2.5, 4, 5].forEach(r => {
        expect(component.getStarArray(r).length).toBe(5);
      });
    });
  });

  describe('resolveImageUrl', () => {
    it('should return empty string for empty url', () => {
      expect(component.resolveImageUrl('')).toBe('');
    });

    it('should return http:// url as-is', () => {
      expect(component.resolveImageUrl('http://example.com/img.jpg')).toBe('http://example.com/img.jpg');
    });

    it('should return https:// url as-is', () => {
      expect(component.resolveImageUrl('https://example.com/img.jpg')).toBe('https://example.com/img.jpg');
    });

    it('should return data: url as-is', () => {
      expect(component.resolveImageUrl('data:image/png;base64,abc')).toBe('data:image/png;base64,abc');
    });

    it('should prepend / for assets/ paths', () => {
      expect(component.resolveImageUrl('assets/img.jpg')).toBe('/assets/img.jpg');
    });

    it('should return relative paths as-is', () => {
      expect(component.resolveImageUrl('some/relative/path.jpg')).toBe('some/relative/path.jpg');
    });
  });

  describe('image modal', () => {
    it('should open image modal with resolved url', () => {
      component.openImageModal('https://example.com/image.jpg');
      expect(component.activeModalImage()).toBe('https://example.com/image.jpg');
    });

    it('should close image modal and set null', () => {
      component.openImageModal('https://example.com/image.jpg');
      component.closeImageModal();
      expect(component.activeModalImage()).toBeNull();
    });
  });
});
