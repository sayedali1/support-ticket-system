using SupportTicketSystem.Domain.Entities;

namespace SupportTicketSystem.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(int id);
    Task<(List<Ticket> Items, int TotalCount)> GetFilteredAsync(
        int? customerId,      
        int? assignedAgentId, 
        string? search,
        string? status,
        string? priority,
        string sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);

    Task AddCommentAsync(Comment comment);
    Task AddActivityLogAsync(TicketActivityLog log);
    Task<List<Comment>> GetCommentsByTicketIdAsync(int ticketId);
    Task<List<TicketActivityLog>> GetActivityLogsByTicketIdAsync(int ticketId);
    Task<Comment?> GetCommentByIdAsync(int commentId);
    Task AddTimeLogAsync(TimeLog timeLog);
    Task<List<TimeLog>> GetTimeLogsByTicketIdAsync(int ticketId);
    Task<List<Ticket>> GetAllForDashboardAsync(int? agentId);
}