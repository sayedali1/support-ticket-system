using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;

namespace SupportTicketSystem.Infrastructure.Persistence.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(List<Ticket> Items, int TotalCount)> GetFilteredAsync(
        int? customerId,
        int? assignedAgentId,
        string? search,
        string? status,
        string? priority,
        string sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Tickets
            .Include(t => t.Customer)
            .Include(t => t.AssignedAgent)
            .AsQueryable();

        // --- Isolation filters (only applied if the service passed a value) ---
        if (customerId.HasValue)
            query = query.Where(t => t.CustomerId == customerId.Value);

        if (assignedAgentId.HasValue)
            query = query.Where(t => t.AssignedAgentId == assignedAgentId.Value);

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.Title.Contains(search) || t.Description.Contains(search));
        }

        // --- Filters ---
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.TicketStatus>(status, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<Domain.Enums.TicketPriority>(priority, out var priorityEnum))
            query = query.Where(t => t.Priority == priorityEnum);

        // --- Total count BEFORE pagination (needed for PagedResultDto.TotalCount) ---
        var totalCount = await query.CountAsync();

        // --- Sorting ---
        query = sortBy.ToLower() switch
        {
            "title" => sortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => sortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => sortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            _ => sortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // --- Pagination ---
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
    public async Task AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
    }

    public async Task AddActivityLogAsync(TicketActivityLog log)
    {
        _context.TicketActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Comment>> GetCommentsByTicketIdAsync(int ticketId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.TicketId == ticketId)
            .ToListAsync();
    }

    public async Task<List<TicketActivityLog>> GetActivityLogsByTicketIdAsync(int ticketId)
    {
        return await _context.TicketActivityLogs
            .Include(a => a.ChangedByUser)
            .Where(a => a.TicketId == ticketId)
            .ToListAsync();
    }

    public async Task<Comment?> GetCommentByIdAsync(int commentId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }
    public async Task AddTimeLogAsync(TimeLog timeLog)
    {
        _context.TimeLogs.Add(timeLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TimeLog>> GetTimeLogsByTicketIdAsync(int ticketId)
    {
        return await _context.TimeLogs
            .Include(tl => tl.Agent)
            .Where(tl => tl.TicketId == ticketId)
            .OrderByDescending(tl => tl.WorkDate)
            .ToListAsync();
    }

    public async Task<List<Ticket>> GetAllForDashboardAsync(int? agentId)
    {
        var query = _context.Tickets
       .Include(t => t.AssignedAgent)
       .Include(t => t.ActivityLogs)  
       .AsQueryable();

        if (agentId.HasValue)
            query = query.Where(t => t.AssignedAgentId == agentId.Value);

        return await query.ToListAsync();
    }
}