import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Wishlist } from './wishlist';
import { WishlistService } from '../../../services/wishlist.service';
import { ProductService } from '../../../services/product.service';
import { CartService } from '../../../services/cart.service';
import { ToastService } from '../../../services/toast.service';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';

describe('Wishlist', () => {
  let component: Wishlist;
  let fixture: ComponentFixture<Wishlist>;
  let mockWishlistService: any;
  let mockProductService: any;
  let mockCartService: any;
  let mockToastService: any;

  const mockWishlistData = {
    items: [
      {
        id: 1,
        variantId: 10,
        productId: 100,
        productName: 'Product 1',
        categoryName: 'Category 1',
        unitPrice: 50,
        imageUrl: 'http://img1.png',
        isInStock: true
      },
      {
        id: 2,
        variantId: 11,
        productId: 101,
        productName: 'Product 2',
        categoryName: null,
        unitPrice: 75,
        imageUrl: '',
        isInStock: false
      }
    ],
    totalItems: 2
  };

  beforeEach(async () => {
    mockWishlistService = {
      getWishlist: vi.fn().mockReturnValue(of(mockWishlistData)),
      wishlistCountSignal: signal(2),
      removeFromWishlist: vi.fn().mockReturnValue(of(null)),
      clearWishlist: vi.fn().mockReturnValue(of(null))
    };

    mockProductService = {};

    mockCartService = {
      addToCart: vi.fn().mockReturnValue(of(null)),
      updateCartCount: vi.fn()
    };

    mockToastService = {
      success: vi.fn(),
      warning: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Wishlist],
      providers: [
        provideRouter([]),
        { provide: WishlistService, useValue: mockWishlistService },
        { provide: ProductService, useValue: mockProductService },
        { provide: CartService, useValue: mockCartService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Wishlist);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and load wishlist items', () => {
    expect(component).toBeTruthy();
    expect(mockWishlistService.getWishlist).toHaveBeenCalled();
    expect(component.wishlistItems().length).toBe(2);
    expect(component.wishlistItems()[0].category).toBe('Category 1');
    expect(component.wishlistItems()[1].category).toBe('Product'); // null fallback
    expect(component.wishlistItems()[1].imageUrl).toBe('https://images.unsplash.com/photo-1596436889106-be35e843f974?w=150&q=80'); // empty fallback
    expect(component.inStockItemsCount()).toBe(1);
  });

  it('should handle wishlist load error', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockWishlistService.getWishlist.mockReturnValue(throwError(() => new Error('API error')));
    component.loadWishlist();
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should remove item from wishlist successfully', () => {
    component.removeItem(1);
    expect(mockWishlistService.removeFromWishlist).toHaveBeenCalledWith(1);
    expect(mockToastService.success).toHaveBeenCalledWith('Item removed from wishlist');
  });

  it('should handle error when removing item from wishlist', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockWishlistService.removeFromWishlist.mockReturnValue(throwError(() => new Error('API error')));
    component.removeItem(1);
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to remove item from wishlist');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should clear wishlist if not empty', () => {
    component.clearWishlist();
    expect(mockWishlistService.clearWishlist).toHaveBeenCalled();
    expect(mockToastService.success).toHaveBeenCalledWith('Wishlist cleared');
  });

  it('should not clear wishlist if empty', () => {
    component.wishlistItems.set([]);
    component.clearWishlist();
    expect(mockWishlistService.clearWishlist).not.toHaveBeenCalled();
  });

  it('should handle error when clearing wishlist', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockWishlistService.clearWishlist.mockReturnValue(throwError(() => new Error('API error')));
    component.clearWishlist();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to clear wishlist');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should not add out-of-stock item to cart', () => {
    const item = component.wishlistItems()[1]; // out of stock
    component.addToCart(item);
    expect(mockToastService.warning).toHaveBeenCalledWith('Item is currently out of stock');
    expect(mockCartService.addToCart).not.toHaveBeenCalled();
  });

  it('should add to cart and remove from wishlist successfully', () => {
    const item = component.wishlistItems()[0]; // in stock
    component.addToCart(item);
    expect(mockCartService.addToCart).toHaveBeenCalledWith({ variantId: item.variantId, quantity: 1 });
    expect(mockWishlistService.removeFromWishlist).toHaveBeenCalledWith(item.id);
    expect(mockToastService.success).toHaveBeenCalledWith(`Added ${item.name} to cart & removed from wishlist`);
  });

  it('should add to cart and handle remove from wishlist failure gracefully', () => {
    const item = component.wishlistItems()[0];
    mockWishlistService.removeFromWishlist.mockReturnValue(throwError(() => new Error('Remove error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    component.addToCart(item);
    expect(mockToastService.success).toHaveBeenCalledWith(`Added ${item.name} to cart`);
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should handle error when adding to cart', () => {
    const item = component.wishlistItems()[0];
    mockCartService.addToCart.mockReturnValue(throwError(() => new Error('Cart error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    component.addToCart(item);
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to add item to cart');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should not add any items to cart if none are in stock when addAllToCart is called', () => {
    component.wishlistItems.set([
      { id: 2, variantId: 11, productId: 101, name: 'Product 2', category: 'Category', price: 10, imageUrl: '', isInStock: false }
    ]);
    component.addAllToCart();
    expect(mockCartService.addToCart).not.toHaveBeenCalled();
  });

  it('should add all in-stock items to cart sequentially', () => {
    // Both items in stock
    component.wishlistItems.set([
      { id: 1, variantId: 10, productId: 100, name: 'Product 1', category: 'Category 1', price: 50, imageUrl: 'img1', isInStock: true },
      { id: 2, variantId: 11, productId: 101, name: 'Product 2', category: 'Category 2', price: 75, imageUrl: 'img2', isInStock: true }
    ]);
    
    component.addAllToCart();
    expect(mockCartService.addToCart).toHaveBeenCalledTimes(2);
    expect(mockWishlistService.removeFromWishlist).toHaveBeenCalledTimes(2);
    expect(mockToastService.success).toHaveBeenCalledWith('Added 2 item(s) to cart & updated wishlist');
  });

  it('should handle add to cart error for some items in addAllToCart gracefully', () => {
    component.wishlistItems.set([
      { id: 1, variantId: 10, productId: 100, name: 'Product 1', category: 'Category 1', price: 50, imageUrl: 'img1', isInStock: true },
      { id: 2, variantId: 11, productId: 101, name: 'Product 2', category: 'Category 2', price: 75, imageUrl: 'img2', isInStock: true }
    ]);

    // Make first one fail
    mockCartService.addToCart.mockReturnValueOnce(throwError(() => new Error('err'))).mockReturnValueOnce(of(null));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

    component.addAllToCart();
    expect(mockCartService.addToCart).toHaveBeenCalledTimes(2);
    expect(mockWishlistService.removeFromWishlist).toHaveBeenCalledTimes(1); // only the second one succeeded
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should handle removeFromWishlist error in addAllToCart gracefully', () => {
    component.wishlistItems.set([
      { id: 1, variantId: 10, productId: 100, name: 'Product 1', category: 'Category 1', price: 50, imageUrl: 'img1', isInStock: true }
    ]);
    mockWishlistService.removeFromWishlist.mockReturnValueOnce(throwError(() => new Error('remove err')));

    component.addAllToCart();
    expect(mockCartService.addToCart).toHaveBeenCalledTimes(1);
    expect(mockWishlistService.removeFromWishlist).toHaveBeenCalledTimes(1);
    expect(mockToastService.success).toHaveBeenCalledWith('Added 1 item(s) to cart & updated wishlist');
  });

  describe('HTML Template rendering and actions', () => {
    it('should render empty state when wishlist has no items', () => {
      component.wishlistItems.set([]);
      fixture.detectChanges();

      const emptyState = fixture.debugElement.nativeElement.querySelector('.empty-wishlist-state');
      expect(emptyState).toBeTruthy();
    });

    it('should render items table and trigger addToCart click on click button', () => {
      component.wishlistItems.set([
        { id: 1, variantId: 10, productId: 100, name: 'Product 1', category: 'Category 1', price: 50, imageUrl: 'img1', isInStock: true }
      ]);
      fixture.detectChanges();

      const table = fixture.debugElement.nativeElement.querySelector('.wishlist-table');
      expect(table).toBeTruthy();

      const addToCartBtn = table.querySelector('.add-to-cart-btn');
      expect(addToCartBtn).toBeTruthy();

      const spy = vi.spyOn(component, 'addToCart');
      addToCartBtn.click();
      expect(spy).toHaveBeenCalled();
    });
  });
});
