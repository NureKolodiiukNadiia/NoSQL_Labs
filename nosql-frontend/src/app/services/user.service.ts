import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateUserRequest {
  id?: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

export interface UserDto {
  id: string;
  email: string;
  password?: string;
  firstName?: string;
  lastName?: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly baseUrl = 'http://localhost:5086/api/users';

  constructor(private http: HttpClient) {}

  createUser(request: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(this.baseUrl, request);
  }

  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.baseUrl);
  }

  getUserById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.baseUrl}/${id}`);
  }

  updateUser(id: string, request: CreateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
