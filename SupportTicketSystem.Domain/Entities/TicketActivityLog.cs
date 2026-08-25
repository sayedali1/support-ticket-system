namespace SupportTicketSystem.Domain.Entities;

public class TicketActivityLog
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;

    public string FieldChanged { get; set; } = string.Empty; // e.g. "Status", "Priority", "AssignedAgent"
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
