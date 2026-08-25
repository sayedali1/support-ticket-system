using System.ComponentModel.DataAnnotations;
using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; }

    public int CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Optimistic concurrency token
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TicketActivityLog> ActivityLogs { get; set; } = new List<TicketActivityLog>();
    public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}
