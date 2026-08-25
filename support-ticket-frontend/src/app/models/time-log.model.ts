export interface TimeLog {
  id: number;
  agentName: string;
  workDate: string;
  durationMinutes: number;
  description?: string;
}

export interface TimeLogCreateRequest {
  workDate: string;
  durationMinutes: number;
  description?: string;
}

export interface TicketTimeSummary {
  ticketId: number;
  totalMinutes: number;
  entries: TimeLog[];
}