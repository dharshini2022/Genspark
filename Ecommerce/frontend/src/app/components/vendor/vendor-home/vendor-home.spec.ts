import { ComponentFixture, TestBed } from '@angular/core/testing';

import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { VendorHome } from './vendor-home';

describe('VendorHome', () => {
  let component: VendorHome;
  let fixture: ComponentFixture<VendorHome>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorHome],
      providers: [
        provideRouter([]),
        provideHttpClient()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorHome);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
