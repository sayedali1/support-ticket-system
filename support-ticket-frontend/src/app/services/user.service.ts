import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AppUser, UserCreateRequest, UserUpdateRequest, UserSummary } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/Users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AppUser[]> {
    return this.http.get<AppUser[]>(this.apiUrl);
  }

  getAgents(): Observable<UserSummary[]> {
    return this.http.get<UserSummary[]>(`${this.apiUrl}/agents`);
  }

  create(dto: UserCreateRequest): Observable<AppUser> {
    return this.http.post<AppUser>(this.apiUrl, dto);
  }

  update(id: number, dto: UserUpdateRequest): Observable<AppUser> {
    return this.http.put<AppUser>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}