using SupportTicketSystem.Application.DTOs;

namespace SupportTicketSystem.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int requestingUserId, string requestingUserRole);
}