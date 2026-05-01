import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService, CreateUserRequest, UserDto } from '../../services/user.service';
import { WorkspaceService, Workspace, FindNearLocationRequest } from '../../services/workspace.service';
import {BookingService, BookingCountDto, CreateBookingRequest} from '../../services/booking.service';
import {BookingsComponent} from '../bookings-component/bookings-component';
import {UsersComponent} from '../users-component/users-component';
import {WorkspacesComponent} from '../workspaces-component/workspaces-component';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, BookingsComponent, UsersComponent, WorkspacesComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css',
})
export class AdminComponent {
  activeTab = signal<'users' | 'workspaces' | 'bookings'>('users');
}
