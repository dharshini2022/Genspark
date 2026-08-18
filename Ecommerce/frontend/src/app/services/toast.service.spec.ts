import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ToastService]
    });
    service = TestBed.inject(ToastService);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should show success toast by default', () => {
    service.success('Success Message');
    const list = service.toasts();
    expect(list.length).toBe(1);
    expect(list[0].message).toBe('Success Message');
    expect(list[0].type).toBe('success');
    expect(list[0].duration).toBe(3000);

    // Should dismiss automatically after 3000ms
    vi.advanceTimersByTime(3000);
    expect(service.toasts().length).toBe(0);
  });

  it('should show info toast with custom duration', () => {
    service.info('Info Message', 5000);
    const list = service.toasts();
    expect(list[0].type).toBe('info');
    expect(list[0].duration).toBe(5000);

    vi.advanceTimersByTime(4999);
    expect(service.toasts().length).toBe(1);

    vi.advanceTimersByTime(1);
    expect(service.toasts().length).toBe(0);
  });

  it('should show warning toast', () => {
    service.warning('Warning Message');
    expect(service.toasts()[0].type).toBe('warning');
  });

  it('should show error toast', () => {
    service.error('Error Message');
    expect(service.toasts()[0].type).toBe('error');
  });

  it('should dismiss toast manually by id', () => {
    service.success('Msg 1');
    service.success('Msg 2');
    const list = service.toasts();
    expect(list.length).toBe(2);

    const firstId = list[0].id;
    service.dismiss(firstId);

    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].id).not.toBe(firstId);
  });
});
