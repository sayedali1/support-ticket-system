using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Domain.Entities;
using SupportTicketSystem.Domain.Enums;
using SupportTicketSystem.Infrastructure.Persistence;
using Xunit;

namespace SupportTicketSystem.Tests;

public class TicketsControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketsControllerIntegrationTests()
    {
        // Fresh factory + fresh in-memory database for EVERY test method.
        // CustomWebApplicationFactory picks a new Guid-named database each time
        // it's constructed, so tests can never see each other's data.
        _factory = new CustomWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    // Helper: seed 2 customers + 1 ticket owned by customer A, return their IDs
    private async Task<(int customerAId, int customerBId, int ticketId)> SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var customerA = new User { FullName = "Customer A", Email = $"a{Guid.NewGuid()}@test.com", Role = UserRole.Customer, PasswordHash = "x" };
        var customerB = new User { FullName = "Customer B", Email = $"b{Guid.NewGuid()}@test.com", Role = UserRole.Customer, PasswordHash = "x" };
        db.Users.AddRange(customerA, customerB);
        await db.SaveChangesAsync();

        var ticket = new Ticket
        {
            Title = "Test ticket",
            Description = "Test description",
            Status = TicketStatus.Open,
            Priority = TicketPriority.Medium,
            CustomerId = customerA.Id
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        return (customerA.Id, customerB.Id, ticket.Id);
    }

    // Helper: manually build a JWT for a given user, bypassing real login (faster, more direct for tests)
    private async Task<HttpClient> GetAuthenticatedClientAsync(int userId, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupportTicketSystem.Application.Interfaces.ITokenService>();

        var fakeUser = new User { Id = userId, Role = Enum.Parse<UserRole>(role), FullName = "Test", Email = "test@test.com" };
        var token = tokenService.GenerateToken(fakeUser, out _);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task GetTicketById_AsOwningCustomer_Returns200()
    {
        var (customerAId, _, ticketId) = await SeedTestDataAsync();
        var client = await GetAuthenticatedClientAsync(customerAId, "Customer");

        var response = await client.GetAsync($"/api/Tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTicketById_AsDifferentCustomer_Returns404()
    {
        // This is THE data isolation test — hitting the real endpoint, real pipeline, real JWT
        var (_, customerBId, ticketId) = await SeedTestDataAsync();
        var client = await GetAuthenticatedClientAsync(customerBId, "Customer");

        var response = await client.GetAsync($"/api/Tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTicketById_WithoutToken_Returns401()
    {
        var (_, _, ticketId) = await SeedTestDataAsync();
        var client = _factory.CreateClient(); // no auth header at all

        var response = await client.GetAsync($"/api/Tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_AsCustomer_Returns201()
    {
        var (customerAId, _, _) = await SeedTestDataAsync();
        var client = await GetAuthenticatedClientAsync(customerAId, "Customer");

        var dto = new TicketCreateDto { Title = "New issue", Description = "Something is broken", Priority = TicketPriority.High };
        var response = await client.PostAsJsonAsync("/api/Tickets", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_WithEmptyTitle_Returns400()
    {
        // Confirms input validation actually works through the real HTTP pipeline
        var (customerAId, _, _) = await SeedTestDataAsync();
        var client = await GetAuthenticatedClientAsync(customerAId, "Customer");

        var dto = new TicketCreateDto { Title = "", Description = "Valid description here", Priority = TicketPriority.Low };
        var response = await client.PostAsJsonAsync("/api/Tickets", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}