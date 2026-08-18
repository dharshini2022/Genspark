import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CartDiscount } from './cart-discount';
import { DiscountService } from '../../../../services/disocunt.service';
import { ToastService } from '../../../../services/toast.service';
import { SimpleChange } from '@angular/core';
import { of, throwError } from 'rxjs';

describe('CartDiscount', () => {
  let component: CartDiscount;
  let fixture: ComponentFixture<CartDiscount>;
  let mockDiscountService: any;
  let mockToastService: any;

  const mockDiscounts = [
    {
      id: 1,
      code: 'DISC10',
      minOrderValue: 100,
      value: 10,
      type: 'Fixed'
    },
    {
      id: 2,
      code: 'DISC20',
      minOrderValue: 200,
      value: 20,
      type: 'Percentage'
    }
  ];

  beforeEach(async () => {
    mockDiscountService = {
      getApplicableLockedDiscounts: vi.fn().mockReturnValue(of(mockDiscounts))
    };

    mockToastService = {
      success: vi.fn(),
      warning: vi.fn(),
      error: vi.fn()
    };

    // Mock clipboard API
    Object.assign(navigator, {
      clipboard: {
        writeText: vi.fn().mockImplementation(() => Promise.resolve())
      }
    });

    await TestBed.configureTestingModule({
      imports: [CartDiscount],
      providers: [
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CartDiscount);
    component = fixture.componentInstance;
    component.subTotal = 150;
    component.cartItems = [
      { id: 1, productId: 101, categoryId: 10, vendorId: 5, price: 50, quantity: 3, isInStock: true }
    ];
    component.loadApplicableLockedDiscounts();
    fixture.detectChanges();
  });

  it('should create and load discounts', () => {
    expect(component).toBeTruthy();
    expect(mockDiscountService.getApplicableLockedDiscounts).toHaveBeenCalled();
    expect(component.applicableLockedDiscounts()).toEqual(mockDiscounts);
  });

  it('should handle clipboard copy success', async () => {
    const event = new MouseEvent('click');
    const stopPropagationSpy = vi.spyOn(event, 'stopPropagation');
    component.copyToClipboard('DISC10', event);
    expect(stopPropagationSpy).toHaveBeenCalled();
    // Flush promise microtasks
    await new Promise(resolve => setTimeout(resolve, 0));
    expect(navigator.clipboard.writeText).toHaveBeenCalledWith('DISC10');
    expect(mockToastService.success).toHaveBeenCalledWith('Discount code copied to clipboard!');
  });

  it('should handle clipboard copy failure', async () => {
    Object.assign(navigator, {
      clipboard: {
        writeText: vi.fn().mockImplementation(() => Promise.reject('error'))
      }
    });
    const event = new MouseEvent('click');
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    component.copyToClipboard('DISC10', event);
    // Flush promise microtasks
    await new Promise(resolve => setTimeout(resolve, 0));
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to copy discount code.');
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should apply discount code successfully', () => {
    vi.spyOn(component.selectDiscount, 'emit');
    // Discount 1 (minOrderValue 100) is unlocked at subtotal 150
    component.applyDiscountCode(mockDiscounts[0] as any);
    expect(component.selectDiscount.emit).toHaveBeenCalledWith(mockDiscounts[0]);
  });

  it('should toggle off discount code if it matches current applied discount', () => {
    vi.spyOn(component.selectDiscount, 'emit');
    component.appliedDiscountCode = 'DISC10';
    component.applyDiscountCode(mockDiscounts[0] as any);
    expect(component.selectDiscount.emit).toHaveBeenCalledWith(null);
  });

  it('should warn if discount code is still locked', () => {
    vi.spyOn(component.selectDiscount, 'emit');
    // Discount 2 (minOrderValue 200) is locked at subtotal 150
    component.applyDiscountCode(mockDiscounts[1] as any);
    expect(mockToastService.warning).toHaveBeenCalledWith('This discount is still locked. Add more items to unlock it.');
    expect(component.selectDiscount.emit).not.toHaveBeenCalled();
  });

  it('should load applicable locked discounts on ngOnChanges', () => {
    mockDiscountService.getApplicableLockedDiscounts.mockClear();
    component.subTotal = 180;
    component.ngOnChanges({
      subTotal: new SimpleChange(150, 180, false)
    });
    expect(mockDiscountService.getApplicableLockedDiscounts).toHaveBeenCalled();
  });

  it('should not call API if subtotal and itemsKey are same', () => {
    mockDiscountService.getApplicableLockedDiscounts.mockClear();
    // First load is triggered in constructor/ngOnInit/fixture detectChanges.
    // Trigger ngOnChanges with same subTotal and items
    component.ngOnChanges({
      subTotal: new SimpleChange(150, 150, false)
    });
    expect(mockDiscountService.getApplicableLockedDiscounts).not.toHaveBeenCalled();
  });

  it('should clear applicable locked discounts if no items in stock', () => {
    component.cartItems = [{ id: 1, productId: 101, isInStock: false }];
    component.loadApplicableLockedDiscounts();
    expect(component.applicableLockedDiscounts()).toEqual([]);
  });

  it('should log error when getApplicableLockedDiscounts fails', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    mockDiscountService.getApplicableLockedDiscounts.mockReturnValue(throwError(() => new Error('API Error')));
    component.subTotal = 999;
    component.loadApplicableLockedDiscounts();
    expect(consoleSpy).toHaveBeenCalled();
    consoleSpy.mockRestore();
  });

  it('should compute getProgressPercent and getRemainingAmount correctly', () => {
    // Discount with no min order value
    const noMin: any = { code: 'NOMIN', minOrderValue: 0 };
    expect(component.getProgressPercent(noMin)).toBe(100);

    const normal: any = { code: 'NORMAL', minOrderValue: 200 };
    // subTotal = 150, min = 200 => 75%
    expect(component.getProgressPercent(normal)).toBe(75);
    expect(component.getRemainingAmount(normal)).toBe(50);
  });
});
