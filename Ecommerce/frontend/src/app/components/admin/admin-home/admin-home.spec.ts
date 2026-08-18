import { ComponentFixture, TestBed } from '@angular/core/testing';

import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { AdminHome } from './admin-home';

describe('AdminHome', () => {
  let component: AdminHome;
  let fixture: ComponentFixture<AdminHome>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminHome],
      providers: [
        provideRouter([]),
        provideHttpClient()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminHome);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
