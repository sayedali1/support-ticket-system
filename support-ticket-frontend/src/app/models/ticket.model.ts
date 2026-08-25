export type TicketStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';

export interface Ticket {
  id: number;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerId: number;
  customerName: string;
  assignedAgentId?: number | null;
  assignedAgentName?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TicketCreateRequest {
  title: string;
  description: string;
  priority: TicketPriority;
}

export interface TicketUpdateRequest {
  status?: TicketStatus;
  priority?: TicketPriority;
  assignedAgentId?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface TicketQueryParams {
  search?: string;
  status?: TicketStatus;
  priority?: TicketPriority;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}