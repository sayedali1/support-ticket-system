export type UserRole = 'Admin' | 'SupportAgent' | 'Customer';

export interface AppUser {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
}

export interface UserCreateRequest {
  fullName: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UserUpdateRequest {
  fullName?: string;
  role?: UserRole;
}

export interface UserSummary {
  id: number;
  fullName: string;
}