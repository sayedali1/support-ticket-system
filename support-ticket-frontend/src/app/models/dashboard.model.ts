export interface StatusCount {
  status: string;
  count: number;
}

export interface AgentWorkload {
  agentId: number;
  agentName: string;
  assignedTicketCount: number;
  openAssignedCount: number;
}

export interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  openCriticalTickets: number;
  averageResolutionHours: number;
  agentWorkload: AgentWorkload[];
  statusBreakdown: StatusCount[];
}