import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { Transaction } from './transaction';

describe('Transaction', () => {
  let component: Transaction;
  let fixture: ComponentFixture<Transaction>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Transaction],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Transaction);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
