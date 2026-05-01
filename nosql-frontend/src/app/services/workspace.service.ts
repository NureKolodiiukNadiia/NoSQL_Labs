import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Workspace {
  id: string;
  name: string;
  spaceType?: number;
  hourlyRate?: number;
  capacity?: number;
  amenities?: string[];
  isActive: boolean;
  location?: { type: string; coordinates: number[] };
}

export interface CreateWorkspaceRequest {
  name: string;
  spaceType: number;
  hourlyRate: number;
  capacity?: number | null;
  amenities?: string[] | null;
  location?: { type: string; coordinates: number[] } | null;
  isActive?: boolean | null;
}

export interface FindNearLocationRequest {
  x: number;
  y: number;
  radiusKm: number;
}

@Injectable({ providedIn: 'root' })
export class WorkspaceService {
  private readonly baseUrl = 'http://localhost:5086/api/workspaces';

  constructor(private http: HttpClient) {}

  createWorkspace(request: CreateWorkspaceRequest): Observable<void> {
    return this.http.post<void>(this.baseUrl, request);
  }

  getWorkspaces(): Observable<Workspace[]> {
    return this.http.get<Workspace[]>(this.baseUrl);
  }

  getWorkspaceById(id: string): Observable<Workspace> {
    return this.http.get<Workspace>(`${this.baseUrl}/${id}`);
  }

  updateWorkspace(id: string, request: CreateWorkspaceRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  deleteWorkspace(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  findByNamePattern(pattern: string): Observable<Workspace[]> {
    return this.http.get<Workspace[]>(`${this.baseUrl}/by-pattern?pattern=${encodeURIComponent(pattern)}`);
  }

  deactivateMatchingText(textMatch: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/deactivate?textMatch=${encodeURIComponent(textMatch)}`, {});
  }

  appendTextToWorkspaceName(workspaceId: string, suffix: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/append-name?id=${workspaceId}&suffix=${encodeURIComponent(suffix)}`, {});
  }

  appendTextToMultipleByNamePattern(namePattern: string, suffix: string): Observable<{ modifiedCount: number }> {
    return this.http.patch<{ modifiedCount: number }>(
      `${this.baseUrl}/append-name-by-pattern?pattern=${encodeURIComponent(namePattern)}&suffix=${encodeURIComponent(suffix)}`,
      {}
    );
  }

  findNearLocation(request: FindNearLocationRequest): Observable<Workspace[]> {
    return this.http.post<Workspace[]>(`${this.baseUrl}/near`, request);
  }
}
