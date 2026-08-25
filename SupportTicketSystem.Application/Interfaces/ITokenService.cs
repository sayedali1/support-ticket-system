using SupportTicketSystem.Domain.Entities;

namespace SupportTicketSystem.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}