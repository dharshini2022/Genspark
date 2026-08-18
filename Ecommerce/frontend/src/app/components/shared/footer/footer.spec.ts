import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Footer } from './footer';
import { RouterModule } from '@angular/router';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

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

describe('Footer', () => {
  let component: Footer;
  let fixture: ComponentFixture<Footer>;

  beforeEach(async () => {
    sessionStorageMock.clear();
    await TestBed.configureTestingModule({
      imports: [Footer, RouterModule.forRoot([])]
    }).compileComponents();
  });

  afterEach(() => {
    sessionStorageMock.clear();
  });

  const createComponent = () => {
    fixture = TestBed.createComponent(Footer);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should set role to Guest and default routes when no role in sessionStorage', () => {
    createComponent();
    expect(component.role()).toBe('Guest');
    expect(component.dashboardRoute()).toBe('/customer-home');
    expect(component.productsRoute()).toBe('/customer-home/products-list');
    expect(component.profileRoute()).toBe('/login');
  });

  it('should set Admin routes when role is Admin', () => {
    sessionStorageMock.setItem('role', 'Admin');
    createComponent();
    expect(component.role()).toBe('Admin');
    expect(component.dashboardRoute()).toBe('/admin-home/');
    expect(component.productsRoute()).toBe('/admin-home/products-list');
    expect(component.profileRoute()).toBe('/admin-home/profile');
  });

  it('should set Vendor routes when role is Vendor', () => {
    sessionStorageMock.setItem('role', 'Vendor');
    createComponent();
    expect(component.role()).toBe('Vendor');
    expect(component.dashboardRoute()).toBe('/vendor-home/');
    expect(component.productsRoute()).toBe('/vendor-home/products-list');
    expect(component.profileRoute()).toBe('/vendor-home/profile');
  });

  it('should set Customer profile route when role is Customer', () => {
    sessionStorageMock.setItem('role', 'Customer');
    createComponent();
    expect(component.role()).toBe('Customer');
    expect(component.profileRoute()).toBe('/customer-home/profile');
    expect(component.dashboardRoute()).toBe('/customer-home');
    expect(component.productsRoute()).toBe('/customer-home/products-list');
  });
});
