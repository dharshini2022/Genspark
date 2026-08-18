import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SettlementList } from './settlement-list';
import { VendorService } from '../../../services/vendor.service';
import { ToastService } from '../../../services/toast.service';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('SettlementList', () => {
  let component: SettlementList;
  let fixture: ComponentFixture<SettlementList>;
  let mockVendorService: any;
  let mockToastService: any;

  const makePagedResponse = (items: any[] = [], totalCount = 0) => ({
    items,
    totalCount,
    pageNumber: 1,
    pageSize: 10,
    totalPages: Math.ceil(totalCount / 10) || 1,
    hasNext: false,
    hasPrevious: false
  });

  beforeEach(async () => {
    mockVendorService = {
      getMySettlements: vi.fn().mockReturnValue(of(makePagedResponse([{ id: 1, amount: 500 }], 1))),
      getVendorSettlementsById: vi.fn().mockReturnValue(of(makePagedResponse([{ id: 2, amount: 700 }], 1)))
    };

    mockToastService = {
      error: vi.fn(),
      success: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [SettlementList],
      providers: [
        { provide: VendorService, useValue: mockVendorService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();
  });

  const createComponent = () => {
    fixture = TestBed.createComponent(SettlementList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should call getMySettlements by default on init', () => {
    createComponent();
    expect(mockVendorService.getMySettlements).toHaveBeenCalled();
    expect(component.settlements().length).toBe(1);
    expect(component.settlements()[0].id).toBe(1);
  });

  it('should call getVendorSettlementsById when isAdminView=true and vendorId is set', () => {
    fixture = TestBed.createComponent(SettlementList);
    component = fixture.componentInstance;
    component.isAdminView = true;
    component.vendorId = 42;
    fixture.detectChanges();
    expect(mockVendorService.getVendorSettlementsById).toHaveBeenCalledWith(42, 1, 10);
    expect(component.settlements()[0].id).toBe(2);
  });

  it('should show error toast on load failure', () => {
    mockVendorService.getMySettlements.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load settlements list.');
    consoleSpy.mockRestore();
  });

  it('should set loading false after load success', () => {
    createComponent();
    expect(component.loading()).toBe(false);
  });

  it('should set loading false after load error', () => {
    mockVendorService.getMySettlements.mockReturnValue(throwError(() => new Error('fail')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    createComponent();
    expect(component.loading()).toBe(false);
    consoleSpy.mockRestore();
  });

  describe('setPage', () => {
    it('should load settlements for valid page', () => {
      createComponent();
      component.totalCount.set(25);
      fixture.detectChanges();
      mockVendorService.getMySettlements.mockClear();
      component.setPage(2);
      expect(mockVendorService.getMySettlements).toHaveBeenCalledWith(2, 10);
    });

    it('should not load for page less than 1', () => {
      createComponent();
      mockVendorService.getMySettlements.mockClear();
      component.setPage(0);
      expect(mockVendorService.getMySettlements).not.toHaveBeenCalled();
    });

    it('should not load for page greater than totalPages', () => {
      createComponent();
      component.totalCount.set(10);
      mockVendorService.getMySettlements.mockClear();
      component.setPage(5);
      expect(mockVendorService.getMySettlements).not.toHaveBeenCalled();
    });
  });

  describe('toggleSettlementExpand', () => {
    it('should expand a settlement by ID', () => {
      createComponent();
      component.toggleSettlementExpand(10);
      expect(component.expandedSettlementId()).toBe(10);
    });

    it('should collapse an expanded settlement by clicking again', () => {
      createComponent();
      component.toggleSettlementExpand(10);
      component.toggleSettlementExpand(10);
      expect(component.expandedSettlementId()).toBeNull();
    });

    it('should switch to new ID when different ID is expanded', () => {
      createComponent();
      component.toggleSettlementExpand(10);
      component.toggleSettlementExpand(20);
      expect(component.expandedSettlementId()).toBe(20);
    });
  });

  describe('getAppliedDate', () => {
    it('should return a date string', () => {
      createComponent();
      const result = component.getAppliedDate(5);
      expect(typeof result).toBe('string');
      expect(result.length).toBeGreaterThan(0);
    });

    it('should produce different dates for different IDs', () => {
      createComponent();
      const d1 = component.getAppliedDate(1);
      const d2 = component.getAppliedDate(7);
      expect(d1).not.toBe(d2);
    });
  });

  describe('totalPages computed', () => {
    it('should be at least 1', () => {
      createComponent();
      expect(component.totalPages()).toBeGreaterThanOrEqual(1);
    });

    it('should calculate correctly', () => {
      createComponent();
      component.totalCount.set(25);
      expect(component.totalPages()).toBe(3);
    });
  });
});
