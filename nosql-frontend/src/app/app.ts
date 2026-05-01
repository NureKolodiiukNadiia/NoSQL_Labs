import { Component } from '@angular/core';
import { AdminComponent } from './components/admin/admin.component';

@Component({
  selector: 'app-root',
  imports: [AdminComponent],
  template: '<app-admin></app-admin>',
  styleUrl: './app.css'
})
export class App {}
