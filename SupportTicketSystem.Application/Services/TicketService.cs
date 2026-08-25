using Microsoft.Extensions.Logging;
using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Application.Exceptions;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;
namespace SupportTicketSystem.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TicketService> _logger;
    public TicketService(ITicketRepository repository, IUserRepository userRepository, ILogger<TicketService> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _logger = logger;
    }

    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.Open] = new[] { TicketStatus.InProgress },
        [TicketStatus.InProgress] = new[] { TicketStatus.Resolved, TicketStatus.Open },
        [TicketStatus.Resolved] = new[] { TicketStatus.Closed, TicketStatus.InProgress },
        [TicketStatus.Closed] = Array.Empty<TicketStatus>()
    };

    private static bool IsValidTransition(TicketStatus current, TicketStatus next)
    {
        if (current == next) return true;
        return AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);
    }

    public async Task<TicketResponseDto?> GetByIdAsync(int ticketId, int requestingUserId, string requestingUserRole)
    {
        var ticket = await _repository.GetByIdAsync(ticketId);
        if (ticket is null)
            return null;

        if (!CanAccess(ticket, requestingUserId, requestingUserRole))
        {
            _logger.LogWarning(
                "Access denied: User {UserId} ({Role}) attempted to access Ticket {TicketId} without permission",
                requestingUserId, requestingUserRole, ticketId);
            return null;
        }

        return MapToDto(ticket);
    }

    public async Task<PagedResultDto<TicketResponseDto>> GetFilteredAsync(
        TicketQueryParams query, int requestingUserId, string requestingUserRole)
    {
        // This is THE isolation decision point for list queries:
        // decide what restriction (if any) to pass down to the repository, based on role.
        int? customerFilter = requestingUserRole == "Customer" ? requestingUserId : null;
        int? agentFilter = requestingUserRole == "SupportAgent" ? requestingUserId : null;
        // Admin: both stay null → repository applies no restriction → sees everything.

        var (items, totalCount) = await _repository.GetFilteredAsync(
            customerFilter,
            agentFilter,
            query.Search,
            query.Status?.ToString(),
            query.Priority?.ToString(),
            query.SortBy ?? "CreatedAt",
            query.SortDescending,
            query.PageNumber,
            query.PageSize);

        return new PagedResultDto<TicketResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<TicketResponseDto> CreateAsync(TicketCreateDto dto, int customerId)
    {
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = TicketStatus.Open, // always starts Open — client can't set this
            CustomerId = customerId,     // always the logged-in user — client can't set this either
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(ticket);
        return MapToDto(ticket);
    }

    public async Task<TicketResponseDto?> UpdateAsync(
    int ticketId, TicketUpdateDto dto, int requestingUserId, string requestingUserRole)
    {
        var ticket = await _repository.GetByIdAsync(ticketId);
        if (ticket is null)
            return null;

        if (!CanAccess(ticket, requestingUserId, requestingUserRole))
            return null;

        var oldStatus = ticket.Status;
        var oldPriority = ticket.Priority;
        var oldAgentId = ticket.AssignedAgentId;

        if (requestingUserRole == "Customer")
        {
            if (dto.Status.HasValue && dto.Status == TicketStatus.Closed && ticket.Status == TicketStatus.Resolved)
            {
                ticket.Status = TicketStatus.Closed;
            }
        }
        else if (requestingUserRole == "SupportAgent")
        {
            if (dto.Status.HasValue)
            {
                if (!IsValidTransition(ticket.Status, dto.Status.Value))
                    throw new BusinessRuleException(
                        $"Cannot transition ticket from '{ticket.Status}' to '{dto.Status.Value}'.");

                ticket.Status = dto.Status.Value;
            }
        }
        else if (requestingUserRole == "Admin")
        {
            if (dto.Status.HasValue)
            {
                if (!IsValidTransition(ticket.Status, dto.Status.Value))
                    throw new BusinessRuleException(
                        $"Cannot transition ticket from '{ticket.Status}' to '{dto.Status.Value}'.");

                ticket.Status = dto.Status.Value;
            }

            if (dto.Priority.HasValue)
            {
                ticket.Priority = dto.Priority.Value;
            }

            if (dto.AssignedAgentId.HasValue)
            {
                var agent = await _userRepository.GetByIdAsync(dto.AssignedAgentId.Value);

                if (agent is null)
                    throw new BusinessRuleException($"No user found with ID {dto.AssignedAgentId.Value}.");

                if (agent.Role != UserRole.SupportAgent)
                    throw new BusinessRuleException($"User '{agent.FullName}' is not a Support Agent and cannot be assigned tickets.");

                ticket.AssignedAgentId = dto.AssignedAgentId.Value;
            }
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(ticket);

        // --- Auto-log each actual change to the activity timeline ---
        if (oldStatus != ticket.Status)
            await LogChange(ticketId, requestingUserId, "Status", oldStatus.ToString(), ticket.Status.ToString());

        if (oldPriority != ticket.Priority)
            await LogChange(ticketId, requestingUserId, "Priority", oldPriority.ToString(), ticket.Priority.ToString());

        if (oldAgentId != ticket.AssignedAgentId)
            await LogChange(ticketId, requestingUserId, "AssignedAgent", oldAgentId?.ToString(), ticket.AssignedAgentId?.ToString());

        return MapToDto(ticket);
    }

    private async Task LogChange(int ticketId, int userId, string field, string? oldValue, string? newValue)
    {
        await _repository.AddActivityLogAsync(new TicketActivityLog
        {
            TicketId = ticketId,
            ChangedByUserId = userId,
            FieldChanged = field,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAt = DateTime.UtcNow
        });
    }
    public async Task<CommentResponseDto?> AddCommentAsync(
    int ticketId, CommentCreateDto dto, int requestingUserId, string requestingUserRole)
    {
        var ticket = await _repository.GetByIdAsync(ticketId);
        if (ticket is null)
            return null;

        if (!CanAccess(ticket, requestingUserId, requestingUserRole))
            return null;

        var comment = new Comment
        {
            TicketId = ticketId,
            AuthorId = requestingUserId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddCommentAsync(comment);

      
        var savedComment = await _repository.GetCommentByIdAsync(comment.Id);

        return new CommentResponseDto
        {
            Id = savedComment!.Id,
            Content = savedComment.Content,
            AuthorName = savedComment.Author.FullName,
            CreatedAt = savedComment.CreatedAt
        };
    }
    public async Task<List<TimelineEntryDto>?> GetTimelineAsync(
    int ticketId, int requestingUserId, string requestingUserRole)
    {
        var ticket = await _repository.GetByIdAsync(ticketId);
        if (ticket is null)
            return null;

        if (!CanAccess(ticket, requestingUserId, requestingUserRole))
            return null;

        var comments = await _repository.GetCommentsByTicketIdAsync(ticketId);
        var logs = await _repository.GetActivityLogsByTicketIdAsync(ticketId);

        var timeline = new List<TimelineEntryDto>();

        timeline.AddRange(comments.Select(c => new TimelineEntryDto
        {
            Type = "Comment",
            Timestamp = c.CreatedAt,
            AuthorName = c.Author.FullName,
            Content = c.Content
        }));

        timeline.AddRange(logs.Select(a => new TimelineEntryDto
        {
            Type = "ActivityLog",
            Timestamp = a.ChangedAt,
            AuthorName = a.ChangedByUser.FullName,
            FieldChanged = a.FieldChanged,
            OldValue = a.OldValue,
            NewValue = a.NewValue
        }));

        return timeline.OrderBy(t => t.Timestamp).ToList();
    }


    public async Task<TimeLogResponseDto?> AddTimeLogAsync(
    int ticketId, TimeLogCreateDto dto, int requestingUserId, string requestingUserRole)
    {
        var ticket = await _repository.GetByIdAsync(ticketId);
        if (ticket is null)
            return null;

        // Only the assigned agent (or Admin) can log time — a Customer shouldn't be able to
        if (requestingUserRole == "Customer")
            return null;

        if (!CanAccess(ticket, requestingUserId, requestingUserRole))
            return null;

        var timeLog = new TimeLog
        {
            TicketId = ticketId,
            AgentId = requestingUserId,
            WorkDate = dto.WorkDate,
            DurationMinutes = dto.DurationMinutes,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddTimeLogAsync(timeLog);

        // Same reload pattern as comments — need Agent.FullName, which isn't loaded yet
        var logs = await _repository.GetTimeLogsByTicketIdAsync(ticketId);
        var saved = logs.First(l => l.Id == timeLog.Id);

        return new TimeLogResponseDto
        {
            Id = saved.Id,
            AgentName = saved.Agent.FullName,
            WorkDate = saved.WorkDate,
            DurationMinutes = saved.DurationMinutes,
            Description = saved.Description
        };
    }

    public async Task<TicketTimeSummaryDto?> GetTimeSummaryAsync(
        int ticketId, int requestingUserId, string requestingUserRole)
    {
        var ticket = await _repository.GetByIdAsync(ticketId);
        if (ticket is null)
            return null;

        if (!CanAccess(ticket, requestingUserId, requestingUserRole))
            return null;

        var logs = await _repository.GetTimeLogsByTicketIdAsync(ticketId);

        return new TicketTimeSummaryDto
        {
            TicketId = ticketId,
            TotalMinutes = logs.Sum(l => l.DurationMinutes),
            Entries = logs.Select(l => new TimeLogResponseDto
            {
                Id = l.Id,
                AgentName = l.Agent.FullName,
                WorkDate = l.WorkDate,
                DurationMinutes = l.DurationMinutes,
                Description = l.Description
            }).ToList()
        };
    }

    // --- Private helpers ---

    private static bool CanAccess(Ticket ticket, int requestingUserId, string requestingUserRole)
    {
        return requestingUserRole switch
        {
            "Admin" => true,
            "SupportAgent" => ticket.AssignedAgentId == requestingUserId,
            "Customer" => ticket.CustomerId == requestingUserId,
            _ => false
        };
    }

    private static TicketResponseDto MapToDto(Ticket ticket) => new()
    {
        Id = ticket.Id,
        Title = ticket.Title,
        Description = ticket.Description,
        Status = ticket.Status.ToString(),
        Priority = ticket.Priority.ToString(),
        CustomerId = ticket.CustomerId,
        CustomerName = ticket.Customer?.FullName ?? string.Empty,
        AssignedAgentId = ticket.AssignedAgentId,
        AssignedAgentName = ticket.AssignedAgent?.FullName,
        CreatedAt = ticket.CreatedAt,
        UpdatedAt = ticket.UpdatedAt
    };

}