using Microsoft.AspNetCore.Identity;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Domain.Entities;

namespace SupportTicketSystem.Infrastructure.Auth;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);
}