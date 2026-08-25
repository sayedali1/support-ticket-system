import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Ticket,
  TicketCreateRequest,
  TicketUpdateRequest,
  TicketQueryParams,
  PagedResult,
} from '../models/ticket.model';
import { Comment, CommentCreateRequest, TimelineEntry } from '../models/comment.model';
import { TimeLog, TimeLogCreateRequest, TicketTimeSummary } from '../models/time-log.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly apiUrl = `${environment.apiUrl}/Tickets`;

  constructor(private http: HttpClient) {}

  getFiltered(query: TicketQueryParams): Observable<PagedResult<Ticket>> {
    let params = new HttpParams();

    if (query.search) params = params.set('search', query.search);
    if (query.status) params = params.set('status', query.status);
    if (query.priority) params = params.set('priority', query.priority);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending);
    if (query.pageNumber) params = params.set('pageNumber', query.pageNumber);
    if (query.pageSize) params = params.set('pageSize', query.pageSize);

    return this.http.get<PagedResult<Ticket>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.apiUrl}/${id}`);
  }

  create(dto: TicketCreateRequest): Observable<Ticket> {
    return this.http.post<Ticket>(this.apiUrl, dto);
  }

  update(id: number, dto: TicketUpdateRequest): Observable<Ticket> {
    return this.http.put<Ticket>(`${this.apiUrl}/${id}`, dto);
  }

  // --- Comments / Timeline ---

  addComment(ticketId: number, dto: CommentCreateRequest): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${ticketId}/comments`, dto);
  }

  getTimeline(ticketId: number): Observable<TimelineEntry[]> {
    return this.http.get<TimelineEntry[]>(`${this.apiUrl}/${ticketId}/timeline`);
  }

  // --- Time Logs ---

  addTimeLog(ticketId: number, dto: TimeLogCreateRequest): Observable<TimeLog> {
    return this.http.post<TimeLog>(`${this.apiUrl}/${ticketId}/timelogs`, dto);
  }

  getTimeSummary(ticketId: number): Observable<TicketTimeSummary> {
    return this.http.get<TicketTimeSummary>(`${this.apiUrl}/${ticketId}/timelogs`);
  }
}