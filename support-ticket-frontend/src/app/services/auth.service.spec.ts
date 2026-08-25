import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify(); // ensures no unexpected HTTP calls were made
  });

  it('should store token and user info on successful login', () => {
    const mockResponse = {
      token: 'fake-jwt-token',
      email: 'admin@test.com',
      fullName: 'Admin User',
      role: 'Admin',
      expiresAt: '2026-01-01T00:00:00Z',
    };

    service.login({ email: 'admin@test.com', password: 'Admin@123' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/Auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse); // simulate the server responding

    expect(localStorage.getItem('auth_token')).toBe('fake-jwt-token');
    expect(service.getRole()).toBe('Admin');
    expect(service.isLoggedIn()).toBeTrue();
  });

  it('should clear storage on logout', () => {
    localStorage.setItem('auth_token', 'some-token');
    service.logout();
    expect(service.getToken()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('isLoggedIn should return false when no token exists', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });
});