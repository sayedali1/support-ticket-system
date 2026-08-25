using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;

namespace SupportTicketSystem.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // If users already exist, don't reseed (idempotent — safe to call every startup)
        if (await context.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<User>();

        var admin = new User
        {
            FullName = "Admin User",
            Email = "admin@electropi.test",
            Role = UserRole.Admin
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");

        var agent = new User
        {
            FullName = "Sara Agent",
            Email = "agent@electropi.test",
            Role = UserRole.SupportAgent
        };
        agent.PasswordHash = hasher.HashPassword(agent, "Agent@123");

        var customer = new User
        {
            FullName = "Ali Customer",
            Email = "customer@electropi.test",
            Role = UserRole.Customer
        };
        customer.PasswordHash = hasher.HashPassword(customer, "Customer@123");

        context.Users.AddRange(admin, agent, customer);
        await context.SaveChangesAsync();

        // Sample tickets — gives you real data to test filtering/pagination/dashboard later
        var ticket1 = new Ticket
        {
            Title = "Cannot login to portal",
            Description = "Getting 500 error when trying to log in since this morning.",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Critical,
            CustomerId = customer.Id
        };

        var ticket2 = new Ticket
        {
            Title = "Invoice PDF not downloading",
            Description = "Download button does nothing on invoice page.",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.Medium,
            CustomerId = customer.Id,
            AssignedAgentId = agent.Id
        };

        context.Tickets.AddRange(ticket1, ticket2);
        await context.SaveChangesAsync();
    }
}