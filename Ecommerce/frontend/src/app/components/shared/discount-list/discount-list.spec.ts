import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DiscountList } from './discount-list';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';

const sessionStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem(key: string) { return store[key] || null; },
    setItem(key: string, value: string) { store[key] = value.toString(); },
    removeItem(key: string) { delete store[key]; },
    clear() { store = {}; }
  };
})();
Object.defineProperty(globalThis, 'sessionStorage', { value: sessionStorageMock, writable: true });

describe('DiscountList', () => {
  let component: DiscountList;
  let fixture: ComponentFixture<DiscountList>;
  let mockDiscountService: any;
  let mockToastService: any;

  const makePagedResponse = (items: any[] = [], totalCount = 0) => ({
    items,
    totalCount,
    pageNumber: 1,
    pageSize: 10
  });

  const makeDiscount = (overrides = {}) => ({
    id: 1,
    code: 'DISC10',
    scope: 'Common',
    type: 'Flat',
    value: 10,
    minOrderValue: 50,
    usageLimit: 100,
    usageCount: 10,
    expiresAt: '2030-01-01',
    isActive: true,
    ...overrides
  });

  beforeEach(async () => {
    sessionStorageMock.clear();

    mockDiscountService = {
      getDiscountHisotry: vi.fn().mockReturnValue(of(makePagedResponse([makeDiscount()], 1))),
      getMyVendorDiscounts: vi.fn().mockReturnValue(of(makePagedResponse([makeDiscount({ id: 2, code: 'VENDOR10' })], 1))),
      getVendorDiscountsByAdmin: vi.fn().mockReturnValue(of(makePagedResponse([makeDiscount({ id: 3, code: 'ADMIN10' })], 1))),
      deactivateDiscount: vi.fn().mockReturnValue(of({}))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [DiscountList],
      providers: [
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    sessionStorageMock.clear();
  });

  const createComponent = () => {
    fixture = TestBed.createComponent(DiscountList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should set isAdminView=true and call getDiscountHisotry when role is Admin', () => {
    sessionStorageMock.setItem('role', 'Admin');
    createComponent();
    expect(component.isAdminView).toBe(true);
    expect(mockDiscountService.getDiscountHisotry).toHaveBeenCalled();
  });

  it('should call getMyVendorDiscounts when not admin and no vendorId', () => {
    createComponent();
    expect(mockDiscountService.getMyVendorDiscounts).toHaveBeenCalled();
    expect(component.discounts()[0].code).toBe('VENDOR10');
  });

  it('should call getVendorDiscountsByAdmin when isAdminView=true and vendorId set', () => {
    fixture = TestBed.createComponent(DiscountList);
    component = fixture.componentInstance;
    component.isAdminView = true;
    component.vendorId = 99;
    fixture.detectChanges();
    expect(mockDiscountService.getVendorDiscountsByAdmin).toHaveBeenCalledWith(99, 1, 10);
  });

  it('should show error toast on load failure', () => {
    mockDiscountService.getMyVendorDiscounts.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load discounts list.');
    consoleSpy.mockRestore();
  });

  it('should set loading false after success', () => {
    createComponent();
    expect(component.loading()).toBe(false);
  });

  it('should set loading false after error', () => {
    mockDiscountService.getMyVendorDiscounts.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent();
    expect(component.loading()).toBe(false);
    consoleSpy.mockRestore();
  });

  describe('pagination', () => {
    it('should setPage and reload discounts for valid page', () => {
      createComponent();
      component.totalCount.set(25);
      mockVendorDiscountsCallCount(mockDiscountService, true);
      component.setPage(2);
      expect(mockDiscountService.getMyVendorDiscounts).toHaveBeenCalledWith(2, 10);
    });

    it('should not load for out-of-range page (0)', () => {
      createComponent();
      mockDiscountService.getMyVendorDiscounts.mockClear();
      component.setPage(0);
      expect(mockDiscountService.getMyVendorDiscounts).not.toHaveBeenCalled();
    });

    it('should not load for out-of-range page (99)', () => {
      createComponent();
      component.totalCount.set(10); // totalPages = 1
      mockDiscountService.getMyVendorDiscounts.mockClear();
      component.setPage(99);
      expect(mockDiscountService.getMyVendorDiscounts).not.toHaveBeenCalled();
    });

    it('should compute totalPages correctly', () => {
      createComponent();
      component.totalCount.set(25);
      expect(component.totalPages()).toBe(3);
    });
  });

  describe('form visibility', () => {
    it('should show discount form when addDiscount is called', () => {
      createComponent();
      component.addDiscount();
      expect(component.showAddDiscountForm()).toBe(true);
    });

    it('should hide form on onFormCancel', () => {
      createComponent();
      component.addDiscount();
      component.onFormCancel();
      expect(component.showAddDiscountForm()).toBe(false);
    });

    it('should hide form and reload on onFormSuccess', () => {
      createComponent();
      component.addDiscount();
      mockDiscountService.getMyVendorDiscounts.mockClear();
      component.onFormSuccess();
      expect(component.showAddDiscountForm()).toBe(false);
      expect(mockDiscountService.getMyVendorDiscounts).toHaveBeenCalled();
    });
  });

  describe('deactivateDiscount', () => {
    it('should call deactivateDiscount and show success toast', () => {
      vi.spyOn(globalThis, 'confirm' as any).mockReturnValue(true);
      createComponent();
      component.deactivateDiscount('DISC10');
      expect(mockDiscountService.deactivateDiscount).toHaveBeenCalledWith('DISC10');
      expect(mockToastService.success).toHaveBeenCalledWith('Discount coupon "DISC10" deactivated.');
    });

    it('should not call deactivateDiscount if confirm returns false', () => {
      vi.spyOn(globalThis, 'confirm' as any).mockReturnValue(false);
      createComponent();
      component.deactivateDiscount('DISC10');
      expect(mockDiscountService.deactivateDiscount).not.toHaveBeenCalled();
    });

    it('should show error toast if deactivate fails', () => {
      vi.spyOn(globalThis, 'confirm' as any).mockReturnValue(true);
      mockDiscountService.deactivateDiscount.mockReturnValue(throwError(() => new Error('fail')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      createComponent();
      component.deactivateDiscount('DISC10');
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to deactivate coupon.');
      consoleSpy.mockRestore();
    });
  });
});

// Helper: reset mock call count after initial load
function mockVendorDiscountsCallCount(mockDiscountService: any, resetFirst = false) {
  if (resetFirst) mockDiscountService.getMyVendorDiscounts.mockClear();
}
