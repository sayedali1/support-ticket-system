using SupportTicketSystem.Application.DTOs;

namespace SupportTicketSystem.Application.Interfaces;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();
    Task<List<UserSummaryDto>> GetAgentsAsync();
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);
    Task<UserResponseDto?> UpdateAsync(int id, UserUpdateDto dto);
    Task<bool> DeleteAsync(int id, int requestingUserId);
}