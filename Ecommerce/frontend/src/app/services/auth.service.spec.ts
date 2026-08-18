import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTestingController: HttpTestingController;

  const mockPayload = {
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'ADMIN',
    fullName: 'Test Admin',
    exp: 9999999999
  };
  
  const base64Payload = btoa(JSON.stringify(mockPayload));
  const mockToken = `header.${base64Payload}.signature`;

  beforeEach(() => {
    sessionStorage.clear();
    localStorage.clear();
    
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    service = TestBed.inject(AuthService);
    expect(service).toBeTruthy();
  });

  it('should initialize userSubject if user metadata is in sessionStorage', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'Test Admin', role: 'ADMIN' }));
    service = TestBed.inject(AuthService);
    expect(service.currentUserValue).toEqual({ fullName: 'Test Admin', role: 'ADMIN' });
  });

  it('should not initialize userSubject if invalid user JSON is in sessionStorage', () => {
    sessionStorage.setItem('user', 'invalid-json');
    service = TestBed.inject(AuthService);
    expect(service.currentUserValue).toBeNull();
  });

  it('should set userSubject on login', () => {
    service = TestBed.inject(AuthService);
    expect(service.currentUserValue).toBeNull();

    service.login({ email: 'test@example.com', password: 'password' }).subscribe(() => {
      expect(service.currentUserValue).toEqual({ fullName: 'Test Admin', role: 'ADMIN' });
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/login`);
    expect(req.request.method).toBe('POST');
    req.flush({ fullName: 'Test Admin', role: 'ADMIN' });
  });

  it('should not set userSubject on login if response is empty', () => {
    service = TestBed.inject(AuthService);
    service.login({}).subscribe();

    const req = httpTestingController.expectOne(`${service['baseUrl']}/login`);
    req.flush(null);
    expect(service.currentUserValue).toBeNull();
  });

  it('should clear userSubject on logout', () => {
    sessionStorage.setItem('user', JSON.stringify({ fullName: 'Test Admin', role: 'ADMIN' }));
    service = TestBed.inject(AuthService);
    expect(service.currentUserValue).toEqual({ fullName: 'Test Admin', role: 'ADMIN' });

    service.logout({}).subscribe(() => {
      expect(service.currentUserValue).toBeNull();
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/logout`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should call register API', () => {
    service = TestBed.inject(AuthService);
    const mockData = { email: 'user@test.com' };
    service.register(mockData).subscribe(res => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${service['baseUrl']}/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockData);
    req.flush({ success: true });
  });

  describe('decodeToken error cases', () => {
    beforeEach(() => {
      service = TestBed.inject(AuthService);
    });

    it('should return null for malformed tokens', () => {
      expect(service.decodeToken('')).toBeNull();
      expect(service.decodeToken('one.two')).toBeNull();
      expect(service.decodeToken('one.two.three.four')).toBeNull();
    });

    it('should handle decoding failure and try base64 fallback', () => {
      // Create a token where URL decoding fails (e.g. contains invalid % URI sequences)
      // but simple atob base64 decoding works or fails
      const badUriPayload = btoa('{"fullName": "User %1"}');
      const token = `header.${badUriPayload}.signature`;
      expect(service.decodeToken(token)).toEqual({ fullName: 'User %1' });
    });

    it('should return null if base64 fallback also fails', () => {
      const token = 'header.invalid_base64!!!.signature';
      expect(service.decodeToken(token)).toBeNull();
    });

    it('should cover fallback return null when parts length is not 3 in catch', () => {
      let callCount = 0;
      const statefulToken = {
        split: (separator: string) => {
          callCount++;
          if (callCount === 1) {
            return ['header', 'invalid_payload', 'signature'];
          } else {
            return ['header', 'invalid_payload'];
          }
        }
      } as any;
      expect(service.decodeToken(statefulToken)).toBeNull();
    });
  });
});
