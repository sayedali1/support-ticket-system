
using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Application.Exceptions;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Where(u => u.Role != UserRole.Admin).Select(MapToDto).ToList();
    }

    public async Task<List<UserSummaryDto>> GetAgentsAsync()
    {
        var agents = await _userRepository.GetAgentsAsync();
        return agents.Select(a => new UserSummaryDto { Id = a.Id, FullName = a.FullName }).ToList();
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
    {
        if (dto.Role == UserRole.Admin)
            throw new BusinessRuleException("Admin accounts cannot be created through this system. Only one Admin exists.");

        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
            throw new BusinessRuleException($"A user with email '{dto.Email}' already exists.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.Hash(dto.Password);

        await _userRepository.AddAsync(user);
        return MapToDto(user);
    }

    public async Task<UserResponseDto?> UpdateAsync(int id, UserUpdateDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return null;

        if (dto.Role.HasValue && dto.Role.Value == UserRole.Admin)
            throw new BusinessRuleException("Users cannot be promoted to Admin. Only one Admin exists.");

        if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
        if (dto.Role.HasValue) user.Role = dto.Role.Value;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(int id, int requestingUserId) { 
        if (id == requestingUserId)
            throw new BusinessRuleException("You cannot delete your own account while logged in.");

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return false;

        if (user.Role == UserRole.Admin)
            throw new BusinessRuleException("The Admin account cannot be deleted.");

        await _userRepository.DeleteAsync(user);
        return true;
    }

    private static UserResponseDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt
    };
}