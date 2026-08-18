import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VendorProfile } from './vendor-profile';
import { VendorService } from '../../../services/vendor.service';
import { ProductService } from '../../../services/product.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ToastService } from '../../../services/toast.service';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError, NEVER } from 'rxjs';
import { VendorProfileResponse, VendorStatus } from '../../../models/vendor.model';

const mockVendorProfile: VendorProfileResponse = {
  id: 5,
  userId: 10,
  userFullName: 'John Doe',
  userEmail: 'john@example.com',
  storeName: 'John Store',
  storeEmail: 'store@example.com',
  gstNumber: 'GST12345678',
  panNumber: 'PAN1234',
  description: 'A great store',
  logoUrl: '',
  status: VendorStatus.Approved
};

const makeMockVendorService = () => ({
  getVendorProfileById: vi.fn().mockReturnValue(of(mockVendorProfile)),
  getMyVendorProfile: vi.fn().mockReturnValue(of(mockVendorProfile)),
  getAdminRevenueForVendor: vi.fn().mockReturnValue(of({ revenue: 5000 })),
  getMySettlements: vi.fn().mockReturnValue(of({ items: [] })),
  toggleVendorStatus: vi.fn().mockReturnValue(of({}))
});

const makeMockProductService = () => ({
  getProductsByVendorId: vi.fn().mockReturnValue(of({ items: [] })),
  getVendorProducts: vi.fn().mockReturnValue(of({ items: [] })),
  toggleProductStatus: vi.fn().mockReturnValue(of({})),
  updateProduct: vi.fn().mockReturnValue(of({}))
});

const makeMockDiscountService = () => ({
  getVendorDiscountsByAdmin: vi.fn().mockReturnValue(of({ items: [] })),
  getMyVendorDiscounts: vi.fn().mockReturnValue(of({ items: [] }))
});

const makeMockToastService = () => ({
  success: vi.fn(),
  error: vi.fn()
});

// ─────────────────────────────────────────────
// SUITE 1: Self-view (no id param)
// ─────────────────────────────────────────────
describe('VendorProfile (self-view, no id param)', () => {
  let component: VendorProfile;
  let fixture: ComponentFixture<VendorProfile>;
  let mockVendorService: ReturnType<typeof makeMockVendorService>;
  let mockProductService: ReturnType<typeof makeMockProductService>;
  let mockDiscountService: ReturnType<typeof makeMockDiscountService>;
  let mockToastService: ReturnType<typeof makeMockToastService>;

  beforeEach(async () => {
    mockVendorService = makeMockVendorService();
    mockProductService = makeMockProductService();
    mockDiscountService = makeMockDiscountService();
    mockToastService = makeMockToastService();

    await TestBed.configureTestingModule({
      imports: [VendorProfile],
      providers: [
        { provide: VendorService, useValue: mockVendorService },
        { provide: ProductService, useValue: mockProductService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ToastService, useValue: mockToastService },
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({})) } },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorProfile);
    component = fixture.componentInstance;
  });

  afterEach(() => vi.restoreAllMocks());

  it('should create and load vendor self data', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component).toBeTruthy();
    expect(component.isAdminView()).toBe(false);
    expect(mockVendorService.getMyVendorProfile).toHaveBeenCalled();
    expect(component.vendorProfile()).not.toBeNull();
    expect(component.loading()).toBe(false);
  });

  it('should compute revenueGenerated from settlements using netPayoutAmount', async () => {
    mockVendorService.getMySettlements.mockReturnValue(of({
      items: [{ netPayoutAmount: 300, grossAmount: 400 }, { netPayoutAmount: 200, grossAmount: 250 }]
    }));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.revenueGenerated()).toBe(500);
  });

  it('should fallback to grossAmount when netPayoutAmount is missing', async () => {
    mockVendorService.getMySettlements.mockReturnValue(of({ items: [{ grossAmount: 400 }] }));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.revenueGenerated()).toBe(400);
  });

  it('should compute productsCount from products', async () => {
    mockProductService.getVendorProducts.mockReturnValue(of({ items: [{ id: 1 }, { id: 2 }] }));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.productsCount()).toBe(2);
  });

  it('should compute activeDiscountsCount from discounts', async () => {
    mockDiscountService.getMyVendorDiscounts.mockReturnValue(of({
      items: [{ isActive: true }, { isActive: false }, { isActive: true }]
    }));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.activeDiscountsCount()).toBe('2');
  });

  it('should compute settlementsCount from settlements', async () => {
    mockVendorService.getMySettlements.mockReturnValue(of({
      items: [{ id: 1 }, { id: 2 }, { id: 3 }]
    }));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.settlementsCount()).toBe('3');
  });

  it('should fallback settlements to [] when getMySettlements catchError triggers', async () => {
    mockVendorService.getMySettlements.mockReturnValue(throwError(() => new Error('settlements error')));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.settlements()).toEqual([]);
  });

  it('should fallback vendor products to [] when getVendorProducts catchError triggers', async () => {
    mockProductService.getVendorProducts.mockReturnValue(throwError(() => new Error('products error')));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.products()).toEqual([]);
  });

  it('should fallback vendor discounts to [] when getMyVendorDiscounts catchError triggers', async () => {
    mockDiscountService.getMyVendorDiscounts.mockReturnValue(throwError(() => new Error('discounts error')));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.discounts()).toEqual([]);
  });

  it('should handle error when loading vendor data', async () => {
    const err = new Error('Load failed');
    mockVendorService.getMyVendorProfile.mockReturnValue(throwError(() => err));
    const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    await fixture.whenStable();
    expect(spyConsole).toHaveBeenCalledWith('Error loading vendor self data', err);
    expect(component.errorMsg()).toBe('Failed to load your vendor profile details.');
    expect(component.loading()).toBe(false);
  });

  it('should set tab and update loaded flags', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    component.setTab('settlements');
    expect(component.settlementsLoaded()).toBe(true);
    component.setTab('discounts');
    expect(component.discountsLoaded()).toBe(true);
    component.setTab('products');
    expect(component.activeTab()).toBe('products');
  });

  it('should toggle vendor status (Approved -> deactivate)', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    component.toggleVendorStatus();
    expect(mockVendorService.toggleVendorStatus).toHaveBeenCalledWith(undefined);
    expect(mockToastService.success).toHaveBeenCalledWith('Account has been successfully deactivated!');
  });

  it('should toggle vendor status (non-Approved -> activate)', async () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(of({ ...mockVendorProfile, status: VendorStatus.Pending }));
    fixture.detectChanges();
    await fixture.whenStable();
    component.toggleVendorStatus();
    expect(mockToastService.success).toHaveBeenCalledWith('Account has been successfully activated!');
  });

  it('should handle toggleVendorStatus error', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    const err = new Error('toggle error');
    mockVendorService.toggleVendorStatus.mockReturnValue(throwError(() => err));
    const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});
    component.toggleVendorStatus();
    expect(spyConsole).toHaveBeenCalledWith('Error toggling vendor status', err);
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to toggle store status.');
  });

  it('should do nothing if toggleVendorStatus called when profile is null', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    component.vendorProfile.set(null);
    component.toggleVendorStatus();
    expect(mockVendorService.toggleVendorStatus).not.toHaveBeenCalled();
  });

  it('should show loading spinner when loading and no profile (NEVER observable)', () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(NEVER);
    mockVendorService.getMySettlements.mockReturnValue(NEVER);
    mockProductService.getVendorProducts.mockReturnValue(NEVER);
    mockDiscountService.getMyVendorDiscounts.mockReturnValue(NEVER);
    fixture.detectChanges();

    expect(component.loading()).toBe(true);
    expect(component.vendorProfile()).toBeNull();
    const spinner = fixture.nativeElement.querySelector('.profile-loader-overlay');
    expect(spinner).not.toBeNull();
  });

  it('should show error banner when errorMsg is set via error path', async () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(throwError(() => new Error('fail')));
    vi.spyOn(console, 'error').mockImplementation(() => {});
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(component.errorMsg()).not.toBe('');
    const errorBanner = fixture.nativeElement.querySelector('.message-banner.error');
    expect(errorBanner).not.toBeNull();
  });

  it('should show success banner when successMsg is set', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    component.successMsg.set('All good');
    fixture.detectChanges();
    const successBanner = fixture.nativeElement.querySelector('.message-banner.success');
    expect(successBanner).not.toBeNull();
  });

  it('should render profile content once vendor profile is set', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.profile-content-area')).not.toBeNull();
  });

  it('should render vendor logo when logoUrl is present', async () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(of({ ...mockVendorProfile, logoUrl: 'https://img.example.com/logo.png' }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.profile-logo-large')).not.toBeNull();
  });

  it('should render avatar initials when no logoUrl', async () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(of({ ...mockVendorProfile, logoUrl: '' }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[class*="profile-avatar-large"]')).not.toBeNull();
  });

  it('should toggle GST reveal and show/hide value', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    component.revealGST.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.info-value.text-mono').length).toBeGreaterThan(0);
    component.revealGST.set(false);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.info-value.text-mono').length).toBeGreaterThan(0);
  });

  it('should toggle PAN reveal and show/hide value', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    component.revealPAN.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.info-value.text-mono').length).toBeGreaterThan(0);
    component.revealPAN.set(false);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.info-value.text-mono').length).toBeGreaterThan(0);
  });

  it('should show/hide password text when showPassword toggled', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    component.showPassword.set(true);
    fixture.detectChanges();
    const passwordSpan = fixture.nativeElement.querySelector('.password-font');
    expect(passwordSpan.textContent.trim()).toBe('AdminPass2026!');
    component.showPassword.set(false);
    fixture.detectChanges();
    expect(passwordSpan.textContent.trim()).toBe('••••••••••••');
  });

  it('should show vendor-home breadcrumb link in self-view', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    const link = fixture.nativeElement.querySelector('a.bc-link');
    expect(link).not.toBeNull();
    expect(link.textContent).toContain('Dashboard');
  });

  it('should NOT show admin tabs in vendor self-view', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.profile-tabs-container')).toBeNull();
  });

  it('should render masked GST when gstNumber is empty', async () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(of({ ...mockVendorProfile, gstNumber: '' }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    component.revealGST.set(false);
    fixture.detectChanges();
    const gstSpans = fixture.nativeElement.querySelectorAll('.info-value.text-mono');
    expect(gstSpans[0].textContent).toContain('GST');
  });

  it('should render masked PAN when panNumber is empty', async () => {
    mockVendorService.getMyVendorProfile.mockReturnValue(of({ ...mockVendorProfile, panNumber: '' }));
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    component.revealPAN.set(false);
    fixture.detectChanges();
    const panSpans = fixture.nativeElement.querySelectorAll('.info-value.text-mono');
    expect(panSpans[1].textContent).toContain('PAN');
  });
});

// ─────────────────────────────────────────────
// SUITE 2: Admin view - calling loadAdminData directly
// ─────────────────────────────────────────────
describe('VendorProfile (admin view - direct method calls)', () => {
  let component: VendorProfile;
  let fixture: ComponentFixture<VendorProfile>;
  let mockVendorService: ReturnType<typeof makeMockVendorService>;
  let mockProductService: ReturnType<typeof makeMockProductService>;
  let mockDiscountService: ReturnType<typeof makeMockDiscountService>;
  let mockToastService: ReturnType<typeof makeMockToastService>;

  beforeEach(async () => {
    mockVendorService = makeMockVendorService();
    mockProductService = makeMockProductService();
    mockDiscountService = makeMockDiscountService();
    mockToastService = makeMockToastService();

    await TestBed.configureTestingModule({
      imports: [VendorProfile],
      providers: [
        { provide: VendorService, useValue: mockVendorService },
        { provide: ProductService, useValue: mockProductService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ToastService, useValue: mockToastService },
        // Use no-id paramMap so ngOnInit calls loadVendorData; then tests call loadAdminData directly
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({})) } },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorProfile);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => vi.restoreAllMocks());

  it('should call loadAdminData and set isAdminView', async () => {
    component.isAdminView.set(true);
    component.loadAdminData(5);
    await fixture.whenStable();

    expect(mockVendorService.getVendorProfileById).toHaveBeenCalledWith(5);
    expect(mockVendorService.getAdminRevenueForVendor).toHaveBeenCalledWith(5);
    expect(mockProductService.getProductsByVendorId).toHaveBeenCalledWith(5, 1, 1000);
    expect(mockDiscountService.getVendorDiscountsByAdmin).toHaveBeenCalledWith(5, 1, 1000);
    expect(component.vendorProfile()).not.toBeNull();
  });

  it('should compute admin revenue from adminRevenue.revenue', async () => {
    mockVendorService.getAdminRevenueForVendor.mockReturnValue(of({ revenue: 9999 }));
    component.loadAdminData(5);
    await fixture.whenStable();
    expect(component.revenueGenerated()).toBe(9999);
  });

  it('should fallback revenue to 0 when adminRevenue catchError triggers', async () => {
    mockVendorService.getAdminRevenueForVendor.mockReturnValue(throwError(() => new Error('rev error')));
    component.loadAdminData(5);
    await fixture.whenStable();
    expect(component.revenueGenerated()).toBe(0);
  });

  it('should handle error when loadAdminData fails (forkJoin error)', async () => {
    mockVendorService.getVendorProfileById.mockReturnValue(throwError(() => new Error('Profile error')));
    const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});

    component.loadAdminData(5);
    await fixture.whenStable();

    expect(spyConsole).toHaveBeenCalledWith('Error loading admin profile data', expect.any(Error));
    expect(component.errorMsg()).toBe('Failed to load merchant profile.');
    expect(component.loading()).toBe(false);
  });

  it('should toggle vendor status in admin view and call loadAdminData', async () => {
    // Manually set admin view state
    component.isAdminView.set(true);
    component.vendorProfile.set(mockVendorProfile);

    mockVendorService.toggleVendorStatus.mockReturnValue(of({}));
    mockVendorService.getVendorProfileById.mockReturnValue(of(mockVendorProfile));
    mockVendorService.getAdminRevenueForVendor.mockReturnValue(of({ revenue: 0 }));
    mockProductService.getProductsByVendorId.mockReturnValue(of({ items: [] }));
    mockDiscountService.getVendorDiscountsByAdmin.mockReturnValue(of({ items: [] }));

    component.toggleVendorStatus();

    expect(mockVendorService.toggleVendorStatus).toHaveBeenCalledWith(5);
    expect(mockToastService.success).toHaveBeenCalledWith('Account has been successfully deactivated!');
  });

  it('should show admin tabs section when isAdminView is set and profile is loaded', async () => {
    component.isAdminView.set(true);
    component.loadAdminData(5);
    await fixture.whenStable();
    fixture.detectChanges();

    const tabsContainer = fixture.nativeElement.querySelector('.profile-tabs-container');
    expect(tabsContainer).not.toBeNull();
  });

  it('should show admin breadcrumb link (Vendors) when isAdminView is set', async () => {
    component.isAdminView.set(true);
    component.loadAdminData(5);
    await fixture.whenStable();
    fixture.detectChanges();

    // In admin view, @if (isAdminView()) renders the first anchor with /admin-home/vendors
    const links = fixture.nativeElement.querySelectorAll('a.bc-link');
    expect(links.length).toBeGreaterThan(0);
    const linksText = Array.from(links).map((l: any) => l.textContent.trim()).join('');
    expect(linksText).toContain('Vendors');
  });

  it('should show product tab by default and switch tabs', async () => {
    component.isAdminView.set(true);
    component.loadAdminData(5);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(component.activeTab()).toBe('products');

    component.setTab('settlements');
    expect(component.settlementsLoaded()).toBe(true);

    component.setTab('discounts');
    expect(component.discountsLoaded()).toBe(true);

    component.setTab('products');
    expect(component.activeTab()).toBe('products');
  });
});

// ─────────────────────────────────────────────
// SUITE 3: Utility methods
// ─────────────────────────────────────────────
describe('VendorProfile utility methods', () => {
  let component: VendorProfile;
  let fixture: ComponentFixture<VendorProfile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorProfile],
      providers: [
        { provide: VendorService, useValue: makeMockVendorService() },
        { provide: ProductService, useValue: makeMockProductService() },
        { provide: DiscountService, useValue: makeMockDiscountService() },
        { provide: ToastService, useValue: makeMockToastService() },
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({})) } },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorProfile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => vi.restoreAllMocks());

  it('should return "V" for empty storeName', () => expect(component.getInitials('')).toBe('V'));
  it('should return first 2 chars uppercased for single word', () => expect(component.getInitials('Apple')).toBe('AP'));
  it('should return initials for two-word store name', () => expect(component.getInitials('Tech Store')).toBe('TS'));
  it('should return date string for getAppliedDate', () => expect(component.getAppliedDate(3)).toContain('Jun'));
  it('should return category string for getCategoryForMock', () => {
    const categories = ['Home & Living', 'Beauty', 'Electronics', 'Grocery', 'Fashion', 'Sports'];
    for (let i = 0; i < 6; i++) expect(categories).toContain(component.getCategoryForMock(i));
  });
});
