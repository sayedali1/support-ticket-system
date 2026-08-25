using SupportTicketSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SupportTicketSystem.Application.DTOs;

public class TicketCreateDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required.")]
    public TicketPriority Priority { get; set; }
}

    public class TicketUpdateDto
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public int? AssignedAgentId { get; set; }
}

public class TicketResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public int? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class TicketQueryParams
{
    public string? Search { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}


public class CommentCreateDto
{
    [Required(ErrorMessage = "Comment content is required.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters.")]
    public string Content { get; set; } = string.Empty;
}

public class TimelineEntryDto
{
    public string Type { get; set; } = string.Empty; // "Comment" or "ActivityLog"
    public DateTime Timestamp { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    // Comment-specific
    public string? Content { get; set; }

    // ActivityLog-specific
    public string? FieldChanged { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TimeLogCreateDto
{
    [Required]
    public DateOnly WorkDate { get; set; }

    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes (24 hours).")]
    public int DurationMinutes { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class TimeLogResponseDto
{
    public int Id { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateOnly WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
}

public class TicketTimeSummaryDto
{
    public int TicketId { get; set; }
    public int TotalMinutes { get; set; }
    public List<TimeLogResponseDto> Entries { get; set; } = new();
}