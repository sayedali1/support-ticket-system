namespace SupportTicketSystem.Application.Exceptions;

// Thrown for business rule violations — e.g. invalid ticket status transitions
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

// Thrown when a requested resource doesn't exist
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}