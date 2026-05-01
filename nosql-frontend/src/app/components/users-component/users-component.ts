import {ChangeDetectorRef, Component, inject, signal} from '@angular/core';
import {CreateUserRequest, UserDto, UserService} from '../../services/user.service';
import {FormsModule} from '@angular/forms';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef} from 'ag-grid-community';

@Component({
  selector: 'app-users-component',
  imports: [
    FormsModule,
    AgGridAngular
  ],
  templateUrl: './users-component.html',
  styleUrl: './users-component.css',
})
export class UsersComponent {
  newUser: CreateUserRequest = { email: '', password: '', firstName: '', lastName: '' };
  userId = '';
  userMessage = signal('');
  retrievedUser = signal<UserDto | null>(null);
  users = signal<UserDto[]>([]);
  selectedUser: UserDto | null = null;
  updateUserModel: CreateUserRequest = { email: '', password: '', firstName: '', lastName: '' };

  userColumnDefs: ColDef<UserDto>[] = [
    { field: 'id', headerName: 'ID', flex: 2, minWidth: 180 },
    { field: 'email', headerName: 'Email', flex: 2, minWidth: 160 },
    { field: 'password', headerName: 'Password', flex: 2, minWidth: 160 },
    { field: 'firstName', headerName: 'First Name', flex: 1 },
    { field: 'lastName', headerName: 'Last Name', flex: 1 }
  ];

  userService = inject(UserService);

  createUser() {
    this.userService.createUser(this.newUser).subscribe({
      next: (res) => {
        this.userMessage.set('User created successfully');
        this.newUser = { email: '', password: '', firstName: '', lastName: '' };
      },
      error: (err) => this.userMessage.set('Error: ' + err.message)
    });
  }

  getUser() {
    if (!this.userId) return;
    this.userService.getUserById(this.userId).subscribe({
      next: (res) => this.retrievedUser.set(res),
      error: () => {
        this.retrievedUser.set(null);
        this.userMessage.set('User not found');
      }
    });
  }
  loadUsers() {
    this.userService.getUsers().subscribe({
      next: (res) => {
        this.users.set(res)
      },
      error: (err) => this.userMessage.set('Failed to load users: ' + err.message)
    });
  }

  onSelectUser(event: any) {
    const selected = event.api.getSelectedRows()[0] as UserDto | undefined;
    this.selectedUser = selected ?? null;
    if (selected) {
      this.updateUserModel = {
        email: selected.email,
        password: selected.password ?? '',
        firstName: selected.firstName ?? '',
        lastName: selected.lastName ?? ''
      };
    }
  }

  updateUser() {
    if (!this.selectedUser) return;
    this.userService.updateUser(this.selectedUser.id, this.updateUserModel).subscribe({
      next: () => {
        this.userMessage.set('User updated');
        this.loadUsers();
      },
      error: (err) => this.userMessage.set('Failed to update: ' + err.message)
    });
  }

  deleteSelectedUser() {
    if (!this.selectedUser) return;
    this.userService.deleteUser(this.selectedUser.id).subscribe({
      next: () => {
        this.userMessage.set('User deleted');
        this.selectedUser = null;
        this.loadUsers();
      },
      error: (err) => this.userMessage.set('Failed to delete: ' + err.message)
    });
  }
}
