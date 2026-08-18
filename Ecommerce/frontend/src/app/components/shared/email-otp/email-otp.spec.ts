import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmailOtp } from './email-otp';

describe('EmailOtp', () => {
  let component: EmailOtp;
  let fixture: ComponentFixture<EmailOtp>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmailOtp],
    }).compileComponents();

    fixture = TestBed.createComponent(EmailOtp);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
