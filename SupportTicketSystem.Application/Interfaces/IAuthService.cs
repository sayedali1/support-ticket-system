using SupportTicketSystem.Application.DTOs;

namespace SupportTicketSystem.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}