namespace SupportTicketSystem.Domain.Entities;

public class TimeLog
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public int AgentId { get; set; }
    public User Agent { get; set; } = null!;

    public DateOnly WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
