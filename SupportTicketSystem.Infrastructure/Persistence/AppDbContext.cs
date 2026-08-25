using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Domain.Entities;
using System.Reflection.Emit;

namespace SupportTicketSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<TicketActivityLog> TicketActivityLogs => Set<TicketActivityLog>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

     
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Customer)
            .WithMany(u => u.TicketsCreated)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedAgent)
            .WithMany(u => u.TicketsAssigned)
            .HasForeignKey(t => t.AssignedAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Comment ---
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a ticket deletes its comments

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- TicketActivityLog ---
        modelBuilder.Entity<TicketActivityLog>()
            .HasOne(a => a.Ticket)
            .WithMany(t => t.ActivityLogs)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketActivityLog>()
            .HasOne(a => a.ChangedByUser)
            .WithMany()
            .HasForeignKey(a => a.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- TimeLog ---
        modelBuilder.Entity<TimeLog>()
            .HasOne(tl => tl.Ticket)
            .WithMany(t => t.TimeLogs)
            .HasForeignKey(tl => tl.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TimeLog>()
            .HasOne(tl => tl.Agent)
            .WithMany(u => u.TimeLogs)
            .HasForeignKey(tl => tl.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Constraints ---
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}