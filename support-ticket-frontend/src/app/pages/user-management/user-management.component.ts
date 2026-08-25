import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { UserService } from '../../services/user.service';
import { AppUser, UserRole } from '../../models/user.model';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatCardModule,
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css',
})
export class UserManagementComponent implements OnInit {
  users = signal<AppUser[]>([]);
  isLoading = signal(true);
  errorMessage = signal('');

  displayedColumns: string[] = ['fullName', 'email', 'role', 'createdAt', 'actions'];

  showCreateForm = signal(false);
  createForm: FormGroup;

  editingUserId = signal<number | null>(null);
  editForm: FormGroup;

  constructor(private userService: UserService, private fb: FormBuilder) {
    this.createForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['', Validators.required],
    });

    this.editForm = this.fb.group({
      fullName: ['', Validators.required],
      role: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.userService.getAll().subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to load users.');
        this.isLoading.set(false);
      },
    });
  }

  toggleCreateForm(): void {
    this.showCreateForm.set(!this.showCreateForm());
    this.createForm.reset();
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.userService.create(this.createForm.value).subscribe({
      next: () => {
        this.showCreateForm.set(false);
        this.createForm.reset();
        this.loadUsers();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to create user.');
      },
    });
  }

  startEdit(user: AppUser): void {
    this.editingUserId.set(user.id);
    this.editForm.setValue({ fullName: user.fullName, role: user.role });
  }

  cancelEdit(): void {
    this.editingUserId.set(null);
  }

  submitEdit(id: number): void {
    if (this.editForm.invalid) return;

    this.errorMessage.set('');
    this.userService.update(id, this.editForm.value).subscribe({
      next: () => {
        this.editingUserId.set(null);
        this.loadUsers();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to update user.');
      },
    });
  }

  deleteUser(user: AppUser): void {
    if (!confirm(`Delete ${user.fullName}? This cannot be undone.`)) return;

    this.errorMessage.set('');
    this.userService.delete(user.id).subscribe({
      next: () => this.loadUsers(),
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to delete user.');
      },
    });
  }
}