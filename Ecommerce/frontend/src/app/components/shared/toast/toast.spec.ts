import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Toast } from './toast';
import { ToastService } from '../../../services/toast.service';
import { signal } from '@angular/core';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('Toast', () => {
  let component: Toast;
  let fixture: ComponentFixture<Toast>;
  let mockToastService: any;

  beforeEach(async () => {
    const toastsSignal = signal<any[]>([]);
    mockToastService = {
      toasts: toastsSignal,
      dismiss: vi.fn(),
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Toast],
      providers: [{ provide: ToastService, useValue: mockToastService }]
    }).compileComponents();

    fixture = TestBed.createComponent(Toast);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should assign toasts signal from service', () => {
    expect(component.toasts).toBe(mockToastService.toasts);
  });

  it('should call dismiss on toastService when dismissToast is called', () => {
    component.dismissToast(42);
    expect(mockToastService.dismiss).toHaveBeenCalledWith(42);
  });

  it('should reflect toasts from service signal', () => {
    const sampleToast = { id: 1, message: 'Hello', type: 'success' };
    mockToastService.toasts.set([sampleToast]);
    fixture.detectChanges();
    expect(component.toasts().length).toBe(1);
    expect(component.toasts()[0].message).toBe('Hello');
  });

  it('should dismiss different toast IDs correctly', () => {
    component.dismissToast(1);
    component.dismissToast(2);
    expect(mockToastService.dismiss).toHaveBeenCalledTimes(2);
    expect(mockToastService.dismiss).toHaveBeenCalledWith(1);
    expect(mockToastService.dismiss).toHaveBeenCalledWith(2);
  });
});
