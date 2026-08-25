import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { TicketService } from '../../services/ticket.service';
import { AuthService } from '../../services/auth.service';
import { Ticket, TicketStatus, TicketPriority } from '../../models/ticket.model';
import { TimelineEntry } from '../../models/comment.model';
import { TicketTimeSummary } from '../../models/time-log.model';
import { UserService } from '../../services/user.service';
import { UserSummary } from '../../models/user.model';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatButtonModule, MatChipsModule,
    MatFormFieldModule, MatSelectModule, MatInputModule, MatDividerModule,
    MatIconModule, MatDatepickerModule, MatNativeDateModule,
  ],
  templateUrl: './ticket-detail.component.html',
  styleUrl: './ticket-detail.component.css',
})
export class TicketDetailComponent implements OnInit {
  ticket = signal<Ticket | null>(null);
  timeline = signal<TimelineEntry[]>([]);
  timeSummary = signal<TicketTimeSummary | null>(null);

  isLoading = signal(true);
  errorMessage = signal('');

  newComment = '';
  newTimeLogDate: Date = new Date();
  newTimeLogMinutes = 0;
  newTimeLogDescription = '';

  selectedStatus: TicketStatus | '' = '';
  selectedPriority: TicketPriority | '' = '';
  assignedAgentId: number | null = null;

  ticketId!: number;
  userRole: string | null = '';
  agents = signal<UserSummary[]>([]);
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ticketService: TicketService,
    private authService: AuthService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getRole();
    this.ticketId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadTicket();

   if (this.userRole === 'Admin') {
    this.loadAgents();
  }
}

loadAgents(): void {
  this.userService.getAgents().subscribe({
    next: (agents) => this.agents.set(agents),
    error: (err) => console.error('Failed to load agents', err),
  });

  }

  loadTicket(): void {
    this.isLoading.set(true);

    this.ticketService.getById(this.ticketId).subscribe({
      next: (ticket) => {
        this.ticket.set(ticket);
        this.selectedStatus = ticket.status;
        this.selectedPriority = ticket.priority;
        this.assignedAgentId = ticket.assignedAgentId ?? null;
        this.isLoading.set(false);
        this.loadTimeline();
        if (this.userRole === 'Admin' || this.userRole === 'SupportAgent') {
          this.loadTimeSummary();
        }
      },
      error: () => {
        this.errorMessage.set('Ticket not found or you do not have access.');
        this.isLoading.set(false);
      },
    });
  }

  loadTimeline(): void {
    this.ticketService.getTimeline(this.ticketId).subscribe({
      next: (entries) => this.timeline.set(entries),
      error: (err) => console.error('Failed to load timeline', err),
    });
  }

  loadTimeSummary(): void {
    this.ticketService.getTimeSummary(this.ticketId).subscribe({
      next: (summary) => this.timeSummary.set(summary),
      error: (err) => console.error('Failed to load time summary', err),
    });
  }

  submitComment(): void {
    if (!this.newComment.trim()) return;

    this.ticketService.addComment(this.ticketId, { content: this.newComment }).subscribe({
      next: () => {
        this.newComment = '';
        this.loadTimeline();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to add comment.');
      },
    });
  }

  updateTicket(): void {
    this.errorMessage.set('');

    const payload: any = {};
    if (this.selectedStatus) payload.status = this.selectedStatus;

    if (this.userRole === 'Admin') {
      if (this.selectedPriority) payload.priority = this.selectedPriority;
      if (this.assignedAgentId) payload.assignedAgentId = this.assignedAgentId;
    }

    this.ticketService.update(this.ticketId, payload).subscribe({
      next: (updated) => {
        this.ticket.set(updated);
        this.loadTimeline();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Update failed.');
      },
    });
  }

  closeResolvedTicket(): void {
    this.ticketService.update(this.ticketId, { status: 'Closed' }).subscribe({
      next: (updated) => {
        this.ticket.set(updated);
        this.loadTimeline();
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to close ticket.');
      },
    });
  }

  submitTimeLog(): void {
    if (this.newTimeLogMinutes <= 0) return;

    const workDate = this.newTimeLogDate.toISOString().split('T')[0];

    this.ticketService
      .addTimeLog(this.ticketId, {
        workDate,
        durationMinutes: this.newTimeLogMinutes,
        description: this.newTimeLogDescription || undefined,
      })
      .subscribe({
        next: () => {
          this.newTimeLogMinutes = 0;
          this.newTimeLogDescription = '';
          this.loadTimeSummary();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Failed to log time.');
        },
      });
  }

  
  goBack(): void {
    this.router.navigate(['/tickets']);
  }
}