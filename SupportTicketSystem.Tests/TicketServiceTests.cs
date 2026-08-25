using Moq;
using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Application.Interfaces;
using SupportTicketSystem.Application.Services;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SupportTicketSystem.Tests;

public class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _mockRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ILogger<TicketService>> _mockLogger;
    private readonly TicketService _service;

    public TicketServiceTests()
    {
        _mockRepo = new Mock<ITicketRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<TicketService>>();
        _service = new TicketService(_mockRepo.Object, _mockUserRepo.Object, _mockLogger.Object);
    }

    // --- Data isolation tests ---

    [Fact]
    public async Task GetByIdAsync_CustomerRequestingOwnTicket_ReturnsTicket()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Title = "Test",
            Description = "Desc",
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        // Act
        var result = await _service.GetByIdAsync(ticketId: 1, requestingUserId: 100, requestingUserRole: "Customer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_CustomerRequestingAnotherCustomersTicket_ReturnsNull()
    {
        // Arrange — ticket belongs to customer 100, but customer 200 is asking
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Title = "Test",
            Description = "Desc",
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        // Act
        var result = await _service.GetByIdAsync(ticketId: 1, requestingUserId: 200, requestingUserRole: "Customer");

        // Assert — this is THE core isolation rule the spec cares about
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_AgentRequestingUnassignedTicket_ReturnsNull()
    {
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            AssignedAgentId = 999,
            Title = "Test",
            Description = "Desc",
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _service.GetByIdAsync(ticketId: 1, requestingUserId: 555, requestingUserRole: "SupportAgent");

        Assert.Null(result); // agent 555 is not assigned to this ticket (999 is)
    }

    [Fact]
    public async Task GetByIdAsync_AdminRequestingAnyTicket_ReturnsTicket()
    {
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Title = "Test",
            Description = "Desc",
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _service.GetByIdAsync(ticketId: 1, requestingUserId: 999, requestingUserRole: "Admin");

        Assert.NotNull(result); // Admin bypasses ownership checks entirely
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentTicket_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Ticket?)null);

        var result = await _service.GetByIdAsync(ticketId: 999, requestingUserId: 1, requestingUserRole: "Admin");

        Assert.Null(result);
    }

    // --- Transition validation tests ---

    [Fact]
    public async Task UpdateAsync_InvalidTransition_ThrowsBusinessRuleException()
    {
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Status = TicketStatus.Open,
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        var dto = new TicketUpdateDto { Status = TicketStatus.Closed }; // Open -> Closed directly is invalid

        await Assert.ThrowsAsync<SupportTicketSystem.Application.Exceptions.BusinessRuleException>(
            () => _service.UpdateAsync(1, dto, requestingUserId: 1, requestingUserRole: "Admin"));
    }

    [Fact]
    public async Task UpdateAsync_ValidTransition_Succeeds()
    {
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Status = TicketStatus.Open,
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        var dto = new TicketUpdateDto { Status = TicketStatus.InProgress }; // Open -> InProgress is valid

        var result = await _service.UpdateAsync(1, dto, requestingUserId: 1, requestingUserRole: "Admin");

        Assert.NotNull(result);
        Assert.Equal("InProgress", result!.Status);
    }

    [Fact]
    public async Task UpdateAsync_CustomerCannotChangePriority()
    {
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Status = TicketStatus.Resolved,
            Priority = TicketPriority.Low,
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        var dto = new TicketUpdateDto { Priority = TicketPriority.Critical }; // Customer tries to escalate priority

        var result = await _service.UpdateAsync(1, dto, requestingUserId: 100, requestingUserRole: "Customer");

        Assert.NotNull(result);
        Assert.Equal("Low", result!.Priority); // unchanged — customers can't set priority
    }

    [Fact]
    public async Task UpdateAsync_CustomerCanCloseResolvedTicket()
    {
        var ticket = new Ticket
        {
            Id = 1,
            CustomerId = 100,
            Status = TicketStatus.Resolved,
            Customer = new User { Id = 100, FullName = "Ali" }
        };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ticket);

        var dto = new TicketUpdateDto { Status = TicketStatus.Closed };

        var result = await _service.UpdateAsync(1, dto, requestingUserId: 100, requestingUserRole: "Customer");

        Assert.NotNull(result);
        Assert.Equal("Closed", result!.Status);
    }
}