using SupportTicketSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SupportTicketSystem.Application.DTOs;

public class UserResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserCreateDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}

public class UserUpdateDto
{
    public string? FullName { get; set; }
    public UserRole? Role { get; set; }
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
}