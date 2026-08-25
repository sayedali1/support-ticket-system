namespace SupportTicketSystem.Domain.Enums;

public enum UserRole
{
    Admin,
    SupportAgent,
    Customer
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}
