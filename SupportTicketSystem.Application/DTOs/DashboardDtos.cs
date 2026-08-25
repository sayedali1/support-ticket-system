namespace SupportTicketSystem.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int OpenCriticalTickets { get; set; }
    public double AverageResolutionHours { get; set; }

    public List<AgentWorkloadDto> AgentWorkload { get; set; } = new();
    public List<StatusCountDto> StatusBreakdown { get; set; } = new(); 
}

public class AgentWorkloadDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int AssignedTicketCount { get; set; }
    public int OpenAssignedCount { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}