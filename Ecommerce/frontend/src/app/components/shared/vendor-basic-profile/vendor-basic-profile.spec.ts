import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VendorBasicProfile } from './vendor-basic-profile';
import { VendorBasicResponse } from '../../../models/vendor.model';
import { RouterModule } from '@angular/router';
import { describe, it, expect, beforeEach } from 'vitest';

const mockVendor: VendorBasicResponse = {
  storeName: 'Test Store',
  storeEmail: 'store@test.com',
  description: 'A wonderful test store',
  logoUrl: 'https://example.com/logo.png'
};

describe('VendorBasicProfile', () => {
  let component: VendorBasicProfile;
  let fixture: ComponentFixture<VendorBasicProfile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorBasicProfile, RouterModule.forRoot([])]
    }).compileComponents();
  });

  const createComponent = (vendorInput?: VendorBasicResponse) => {
    fixture = TestBed.createComponent(VendorBasicProfile);
    component = fixture.componentInstance;
    if (vendorInput) {
      component.vendor = vendorInput;
    }
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should have undefined vendor by default', () => {
    createComponent();
    expect(component.vendor).toBeUndefined();
  });

  it('should accept a vendor Input', () => {
    createComponent(mockVendor);
    expect(component.vendor).toBe(mockVendor);
    expect(component.vendor!.storeName).toBe('Test Store');
  });

  it('should render vendor store name in template', () => {
    createComponent(mockVendor);
    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Test Store');
  });

  it('should handle undefined vendor gracefully without error', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should expose vendor storeEmail', () => {
    createComponent(mockVendor);
    expect(component.vendor!.storeEmail).toBe('store@test.com');
  });

  it('should expose vendor description', () => {
    createComponent(mockVendor);
    expect(component.vendor!.description).toBe('A wonderful test store');
  });

  it('should expose vendor logoUrl', () => {
    createComponent(mockVendor);
    expect(component.vendor!.logoUrl).toBe('https://example.com/logo.png');
  });
});
