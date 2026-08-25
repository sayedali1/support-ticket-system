using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Ticket> TicketsCreated { get; set; } = new List<Ticket>();
    public ICollection<Ticket> TicketsAssigned { get; set; } = new List<Ticket>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}
