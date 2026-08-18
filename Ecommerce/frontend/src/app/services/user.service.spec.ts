import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { UserService } from './user.service';

describe('UserService', () => {
  let service: UserService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UserService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(UserService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get profile', () => {
    service.getProfile().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/profile`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('should update profile', () => {
    const payload = { fullName: 'Name', email: 'e@test.com' };
    service.updateProfile(payload).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/updateProfile`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should get my addresses', () => {
    service.getMyAddresses().subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/GetMyAddress`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('should add user address', () => {
    const payload = { recipientName: 'Jane' } as any;
    service.addUserAddress(payload).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/UserAddress`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });

  it('should update user address', () => {
    const payload = { recipientName: 'Jane Updated' } as any;
    service.updateUserAddress(12, payload).subscribe();
    const req = httpTestingController.expectOne(`${service['baseUrl']}/UserAddress/12`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({});
  });
});
