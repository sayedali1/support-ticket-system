import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { TicketService } from '../../services/ticket.service';
import { AuthService } from '../../services/auth.service';
import { Ticket, TicketStatus, TicketPriority } from '../../models/ticket.model';
import { MatCard, MatCardContent } from '@angular/material/card';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatTableModule, MatButtonModule, MatChipsModule,
    MatIconModule, MatPaginatorModule, MatFormFieldModule, MatSelectModule, MatInputModule,
    MatCard,
    MatCardContent
],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.css',
})
export class TicketListComponent implements OnInit {
  tickets = signal<Ticket[]>([]);
  displayedColumns: string[] = ['title', 'status', 'priority', 'customerName', 'assignedAgentName', 'createdAt'];

  totalCount = signal(0);
  pageNumber = 1;
  pageSize = 10;

  searchText = '';
  statusFilter: TicketStatus | '' = '';
  priorityFilter: TicketPriority | '' = '';

  isLoading = signal(false);
  userRole: string | null = '';

  constructor(
    private ticketService: TicketService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getRole();
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading.set(true);

    this.ticketService
      .getFiltered({
        search: this.searchText || undefined,
        status: this.statusFilter || undefined,
        priority: this.priorityFilter || undefined,
        pageNumber: this.pageNumber,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (result) => {
          this.tickets.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load tickets', err);
          this.isLoading.set(false);
        },
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadTickets();
  }

  onFilterChange(): void {
    this.pageNumber = 1;
    this.loadTickets();
  }

  viewTicket(id: number): void {
    this.router.navigate(['/tickets', id]);
  }

  createTicket(): void {
    this.router.navigate(['/tickets/new']);
  }


  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}