import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VendorList } from './vendor-list';
import { VendorService } from '../../../services/vendor.service';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { VendorProfileResponse, VendorStatus } from '../../../models/vendor.model';
import { By } from '@angular/platform-browser';

describe('VendorList', () => {
  let component: VendorList;
  let fixture: ComponentFixture<VendorList>;
  let mockVendorService: any;

  const mockPendingVendors: VendorProfileResponse[] = [
    {
      id: 1,
      storeName: 'Apple Store',
      storeEmail: 'apple@example.com',
      userFullName: 'Steve Jobs',
      userEmail: 'steve@apple.com',
      status: VendorStatus.Pending,
      description: 'Tech store',
      userId: 101,
      gstNumber: 'GST123',
      panNumber: 'PAN123',
      logoUrl: '' // Empty to test initials block
    }
  ];

  const mockApprovedVendors: VendorProfileResponse[] = [
    {
      id: 2,
      storeName: 'Google Store',
      storeEmail: 'google@example.com',
      userFullName: 'Sundar Pichai',
      userEmail: 'sundar@google.com',
      status: VendorStatus.Approved,
      description: 'Search & tech',
      userId: 102,
      gstNumber: 'GST456',
      panNumber: 'PAN456',
      logoUrl: 'google-logo.png' // Non-empty to test logo img tag
    }
  ];

  const mockCancelledVendors: VendorProfileResponse[] = [
    {
      id: 3,
      storeName: 'Cancelled Shop',
      storeEmail: 'cancelled@example.com',
      userFullName: 'Mock Cancel',
      userEmail: 'cancel@example.com',
      status: VendorStatus.Cancelled,
      description: 'Cancelled store',
      userId: 103,
      gstNumber: 'GST789',
      panNumber: 'PAN789',
      logoUrl: 'logo.png'
    }
  ];

  beforeEach(async () => {
    mockVendorService = {
      getVendorsByStatus: vi.fn((status: string) => {
        if (status === 'Pending') return of(mockPendingVendors);
        if (status === 'Approved') return of(mockApprovedVendors);
        return of(mockCancelledVendors);
      }),
      getAllVendors: vi.fn().mockReturnValue(of({
        items: [...mockPendingVendors, ...mockApprovedVendors, ...mockCancelledVendors],
        totalCount: 3
      })),
      approveVendor: vi.fn().mockReturnValue(of({ storeName: 'Apple Store' })),
      cancelVendor: vi.fn().mockReturnValue(of({ storeName: 'Apple Store' }))
    };

    await TestBed.configureTestingModule({
      imports: [VendorList],
      providers: [
        { provide: VendorService, useValue: mockVendorService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorList);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it('should create and load counts and pending vendors by default', () => {
    fixture.detectChanges(); // ngOnInit

    expect(component).toBeTruthy();
    expect(mockVendorService.getVendorsByStatus).toHaveBeenCalledWith('Pending');
    expect(mockVendorService.getVendorsByStatus).toHaveBeenCalledWith('Approved');
    expect(mockVendorService.getVendorsByStatus).toHaveBeenCalledWith('Cancelled');
    
    expect(component.pendingCount()).toBe(1);
    expect(component.activeCount()).toBe(1);
    expect(component.suspendedCount()).toBe(1);
    expect(component.totalCount()).toBe(3);
    
    expect(component.vendors()).toEqual(mockPendingVendors);
    expect(component.loading()).toBe(false);
  });

  it('should handle loadCounts error gracefully', () => {
    mockVendorService.getVendorsByStatus.mockReturnValue(throwError(() => new Error('Error')));
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    
    fixture.detectChanges();
    expect(consoleSpy).toHaveBeenCalled();
  });

  it('should handle loadVendors error gracefully in pending mode', () => {
    mockVendorService.getVendorsByStatus = vi.fn((status: string) => {
      if (status === 'Pending') return throwError(() => new Error('Pending load error'));
      return of([]);
    });
    
    fixture.detectChanges();
    expect(component.errorMsg()).toBe('Failed to load pending vendors list.');
    expect(component.loading()).toBe(false);
  });

  it('should fetch all vendors when activeTab is set to all', () => {
    fixture.detectChanges();
    
    component.setActiveTab('all');
    
    expect(component.activeTab()).toBe('all');
    expect(mockVendorService.getAllVendors).toHaveBeenCalledWith(1, 200);
    expect(component.vendors().length).toBe(3);
  });

  it('should handle loadVendors error in all mode', () => {
    fixture.detectChanges();
    mockVendorService.getAllVendors.mockReturnValue(throwError(() => new Error('All load error')));

    component.setActiveTab('all');
    expect(component.errorMsg()).toBe('Failed to load vendors list.');
    expect(component.loading()).toBe(false);
  });

  it('should open and close review vendor details, testing template clicks', () => {
    fixture.detectChanges();
    const vendor = mockPendingVendors[0];

    component.openReview(vendor);
    fixture.detectChanges();

    expect(component.selectedReviewVendor()).toBe(vendor);

    // Verify stop propagation on modal card click
    const card = fixture.debugElement.query(By.css('.modal-card'));
    card.nativeElement.click();
    fixture.detectChanges();
    expect(component.selectedReviewVendor()).toBe(vendor);

    // Click close button to close
    const closeBtn = fixture.debugElement.query(By.css('.close-btn'));
    closeBtn.nativeElement.click();
    fixture.detectChanges();
    expect(component.selectedReviewVendor()).toBeNull();

    // Reopen and test overlay click to close
    component.openReview(vendor);
    fixture.detectChanges();
    const overlay = fixture.debugElement.query(By.css('.modal-overlay'));
    overlay.nativeElement.click();
    fixture.detectChanges();
    expect(component.selectedReviewVendor()).toBeNull();
  });

  it('should render initials avatar when logoUrl is empty', () => {
    fixture.detectChanges();
    component.openReview(mockPendingVendors[0]);
    fixture.detectChanges();

    const avatar = fixture.debugElement.query(By.css('.merchant-avatar-large'));
    expect(avatar).toBeTruthy();
    expect(avatar.nativeElement.textContent).toContain('AS');
  });

  it('should render image logo when logoUrl is present', () => {
    fixture.detectChanges();
    component.openReview(mockApprovedVendors[0]);
    fixture.detectChanges();

    const logo = fixture.debugElement.query(By.css('.merchant-logo-large'));
    expect(logo).toBeTruthy();
    expect(logo.nativeElement.getAttribute('src')).toBe('google-logo.png');
  });

  it('should render status badge correctly for approved and cancelled statuses', () => {
    fixture.detectChanges();

    component.openReview(mockApprovedVendors[0]);
    fixture.detectChanges();
    let badge = fixture.debugElement.query(By.css('.badge-status'));
    expect(badge.nativeElement.textContent).toContain('Approved');

    component.openReview(mockCancelledVendors[0]);
    fixture.detectChanges();
    badge = fixture.debugElement.query(By.css('.badge-status'));
    expect(badge.nativeElement.textContent).toContain('Rejected');
  });

  it('should approve vendor and handle success with a timeout clear', () => {
    vi.useFakeTimers();
    fixture.detectChanges();
    
    component.approveVendor(1);

    expect(mockVendorService.approveVendor).toHaveBeenCalledWith(1);
    expect(component.successMsg()).toBe('Store "Apple Store" has been approved successfully.');
    expect(component.selectedReviewVendor()).toBeNull();
    expect(component.processingAction()).toBe(false);

    vi.advanceTimersByTime(3000);
    expect(component.successMsg()).toBe('');
  });

  it('should handle approve vendor error', () => {
    fixture.detectChanges();
    mockVendorService.approveVendor.mockReturnValue(throwError(() => new Error('Approve error')));

    component.approveVendor(1);
    expect(component.errorMsg()).toBe('Failed to approve vendor application.');
    expect(component.processingAction()).toBe(false);
  });

  it('should reject/cancel vendor and handle success with a timeout clear', () => {
    vi.useFakeTimers();
    fixture.detectChanges();

    component.rejectVendor(1);

    expect(mockVendorService.cancelVendor).toHaveBeenCalledWith(1);
    expect(component.successMsg()).toBe('Store "Apple Store" has been rejected.');
    expect(component.processingAction()).toBe(false);

    vi.advanceTimersByTime(3000);
    expect(component.successMsg()).toBe('');
  });

  it('should handle reject vendor error', () => {
    fixture.detectChanges();
    mockVendorService.cancelVendor.mockReturnValue(throwError(() => new Error('Reject error')));

    component.rejectVendor(1);
    expect(component.errorMsg()).toBe('Failed to reject vendor application.');
    expect(component.processingAction()).toBe(false);
  });

  it('should compute filteredVendors based on searchTerm', () => {
    fixture.detectChanges();
    component.setActiveTab('all');

    // Case 1: Search by storeName
    component.searchTerm.set('Apple');
    expect(component.filteredVendors().length).toBe(1);
    expect(component.filteredVendors()[0].storeName).toBe('Apple Store');

    // Case 2: Search by storeEmail
    component.searchTerm.set('google.com');
    expect(component.filteredVendors().length).toBe(1);
    expect(component.filteredVendors()[0].storeName).toBe('Google Store');

    // Case 3: Search by userFullName
    component.searchTerm.set('Sundar');
    expect(component.filteredVendors().length).toBe(1);

    // Case 4: Search by userEmail
    component.searchTerm.set('steve@apple.com');
    expect(component.filteredVendors().length).toBe(1);

    // Case 5: Empty search term
    component.searchTerm.set('   ');
    expect(component.filteredVendors().length).toBe(3);
  });

  describe('Helper methods', () => {
    it('should getInitials correctly', () => {
      expect(component.getInitials('')).toBe('V');
      expect(component.getInitials('Microsoft Store')).toBe('MS');
      expect(component.getInitials('Shop')).toBe('SH');
    });

    it('should getAppliedDate correctly', () => {
      const date = component.getAppliedDate(1);
      expect(date).toContain('2026');
      expect(date).toContain('Jun');
    });

    it('should getCategoryForMock correctly', () => {
      const cat = component.getCategoryForMock(1);
      expect(typeof cat).toBe('string');
      expect(cat.length).toBeGreaterThan(0);
    });
  });
});
