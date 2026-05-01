import {Component, inject, signal} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {DatePipe} from '@angular/common';
import {BookingCountDto, BookingDto, BookingService, CreateBookingRequest} from '../../services/booking.service';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef} from 'ag-grid-community';

@Component({
  selector: 'app-bookings-component',
  imports: [
    FormsModule,
    AgGridAngular
  ],
  templateUrl: './bookings-component.html',
  styleUrl: './bookings-component.css',
})
export class BookingsComponent {
  userId = '';
  workspaceId = '';
  startTime = new Date();
  endTime = new Date();
  totalAmount = 0;
  status = '';

  bookingMessage = signal('');
  bookingCounts = signal<BookingCountDto[]>([]);
  bookings = signal<BookingDto[]>([]);
  selectedBooking: BookingDto | null = null;

  bookingColumnDefs: ColDef<BookingDto>[] = [
    { field: 'id', headerName: 'ID', flex: 2, minWidth: 180 },
    { field: 'userId', headerName: 'User ID', flex: 1.5, minWidth: 140},
    { field: 'workspaceId', headerName: 'Workspace ID', flex: 1.5, minWidth: 140 },
    { field: 'startTime', headerName: 'Start', flex: 1.5 },
    { field: 'endTime', headerName: 'End', flex: 1.5 },
    { field: 'totalAmount', headerName: 'Amount', flex: 1 },
    { field: 'status', headerName: 'Status', flex: 1 }
  ];

  bookingService = inject(BookingService);

  loadBookings() {
    this.bookingService.getBookings().subscribe({
      next: (res) => this.bookings.set(res),
      error: (err) => this.bookingMessage.set('Failed to load bookings: ' + err.message)
    });
  }

  createBooking() {
    if (!this.userId || !this.workspaceId) {
      return;
    }

    const convertedStatus = Number(this.status);
    if (Number.isNaN(convertedStatus)) {
      return;
    }

    const req : CreateBookingRequest = {
      userId: this.userId,
      workspaceId: this.workspaceId,
      totalAmount: this.totalAmount,
      status: convertedStatus,
      startTime: new Date(this.startTime),
      endTime: new Date(this.endTime)
    }
    this.bookingService.createBooking(req).subscribe({
      next: () => {
        this.bookingMessage.set('Booking created');
        this.loadBookings();
      },
      error: (err) => this.bookingMessage.set('Failed: ' + err.message)
    });
  }

  onSelectBooking(event: any) {
    const selected = event.api.getSelectedRows()[0] as BookingDto | undefined;
    this.selectedBooking = selected ?? null;
    if (!selected) return;
    this.workspaceId = selected.workspaceId;
    this.userId = selected.userId;
    this.startTime = new Date(selected.startTime);
    this.endTime = new Date(selected.endTime);
    this.totalAmount = selected.totalAmount;
    this.status = selected.status.toString();
  }

  updateSelectedBooking() {
    if (!this.selectedBooking) return;

    const req: CreateBookingRequest = {
      userId: this.userId,
      workspaceId: this.workspaceId,
      startTime: new Date(this.startTime),
      endTime: new Date(this.endTime),
      totalAmount: this.totalAmount,
      status: Number(this.status)
    };

    this.bookingService.updateBooking(this.selectedBooking.id, req).subscribe({
      next: () => {
        this.bookingMessage.set('Booking updated');
        this.loadBookings();
      },
      error: (err) => this.bookingMessage.set('Failed to update booking: ' + err.message)
    });
  }

  deleteSelectedBooking() {
    if (!this.selectedBooking) return;
    this.bookingService.deleteBooking(this.selectedBooking.id).subscribe({
      next: () => {
        this.bookingMessage.set('Booking deleted');
        this.selectedBooking = null;
        this.loadBookings();
      },
      error: (err) => this.bookingMessage.set('Failed to delete booking: ' + err.message)
    });
  }

  getBookingCounts() {
    this.bookingService.getUserBookingCounts().subscribe({
      next: (res) => this.bookingCounts.set(res)
    });
  }
}
