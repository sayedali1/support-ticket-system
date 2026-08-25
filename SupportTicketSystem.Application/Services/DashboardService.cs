using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ITicketRepository _repository;

    public DashboardService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(int requestingUserId, string requestingUserRole)
    {
        int? agentFilter = requestingUserRole == "SupportAgent" ? requestingUserId : null;

        var tickets = await _repository.GetAllForDashboardAsync(agentFilter);

        var resolvedOrClosed = tickets
            .Where(t => t.Status is TicketStatus.Resolved or TicketStatus.Closed)
            .ToList();

        // For each resolved/closed ticket, find the ACTUAL moment it became Resolved
        // from the activity log — not just "last touched" (UpdatedAt).
        var resolutionTimes = new List<double>();
        foreach (var ticket in resolvedOrClosed)
        {
            var resolvedLog = ticket.ActivityLogs
                .Where(a => a.FieldChanged == "Status" && a.NewValue == "Resolved")
                .OrderBy(a => a.ChangedAt)
                .FirstOrDefault();

            if (resolvedLog != null)
            {
                resolutionTimes.Add((resolvedLog.ChangedAt - ticket.CreatedAt).TotalHours);
            }
            // If no log entry exists (e.g. a ticket seeded directly as Resolved with no
            // logged transition), it's excluded from the average rather than guessed at.
        }

        var avgResolutionHours = resolutionTimes.Count > 0 ? resolutionTimes.Average() : 0;

        var stats = new DashboardStatsDto
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == TicketStatus.Open),
            InProgressTickets = tickets.Count(t => t.Status == TicketStatus.InProgress),
            ResolvedTickets = tickets.Count(t => t.Status == TicketStatus.Resolved),
            ClosedTickets = tickets.Count(t => t.Status == TicketStatus.Closed),
            OpenCriticalTickets = tickets.Count(t =>
                t.Status == TicketStatus.Open && t.Priority == TicketPriority.Critical),
            AverageResolutionHours = Math.Round(avgResolutionHours, 1),
            StatusBreakdown = new List<StatusCountDto>
        {
            new() { Status = "Open", Count = tickets.Count(t => t.Status == TicketStatus.Open) },
            new() { Status = "InProgress", Count = tickets.Count(t => t.Status == TicketStatus.InProgress) },
            new() { Status = "Resolved", Count = tickets.Count(t => t.Status == TicketStatus.Resolved) },
            new() { Status = "Closed", Count = tickets.Count(t => t.Status == TicketStatus.Closed) }
        }
        };

        if (requestingUserRole == "Admin")
        {
            stats.AgentWorkload = tickets
                .Where(t => t.AssignedAgentId.HasValue)
                .GroupBy(t => new { t.AssignedAgentId, t.AssignedAgent!.FullName })
                .Select(g => new AgentWorkloadDto
                {
                    AgentId = g.Key.AssignedAgentId!.Value,
                    AgentName = g.Key.FullName,
                    AssignedTicketCount = g.Count(),
                    OpenAssignedCount = g.Count(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress)
                })
                .ToList();
        }

        return stats;
    }
}