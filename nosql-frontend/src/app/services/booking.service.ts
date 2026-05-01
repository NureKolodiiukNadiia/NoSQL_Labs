import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BookingCountDto {
  email: string;
  bookingsCount: number;
}

export interface BookingDto {
  id: string;
  workspaceId: string;
  userId: string;
  startTime: Date;
  endTime: Date;
  totalAmount: number;
  status: number;
}

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly baseUrl = 'http://localhost:5086/api/bookings';

  constructor(private http: HttpClient) {}

  createBooking(req : CreateBookingRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}`, req);
  }

  getBookings(): Observable<BookingDto[]> {
    return this.http.get<BookingDto[]>(this.baseUrl);
  }

  getBookingById(id: string): Observable<BookingDto> {
    return this.http.get<BookingDto>(`${this.baseUrl}/${id}`);
  }

  updateBooking(id: string, req: CreateBookingRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, req);
  }

  deleteBooking(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getUserBookingCounts(): Observable<BookingCountDto[]> {
    return this.http.get<BookingCountDto[]>(`${this.baseUrl}/booking-counts`);
  }
}

export interface CreateBookingRequest {
  workspaceId: string,
  userId: string,
  startTime: Date,
  endTime: Date,
  totalAmount: number,
  status: number
}
/*
public record CreateBookingRequest(
    string WorkspaceId,
    string UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount,
    BookingStatus Status
);
 */
