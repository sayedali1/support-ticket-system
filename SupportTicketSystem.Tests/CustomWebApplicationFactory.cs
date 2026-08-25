using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportTicketSystem.Infrastructure.Persistence;

namespace SupportTicketSystem.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Generated ONCE per factory instance (in the constructor), not inside
    // ConfigureWebHost. WebApplicationFactory can invoke ConfigureWebHost more
    // than once internally when building the host for a minimal-hosting-model
    // app (Program.cs top-level statements) — if the db name were generated
    // fresh each time ConfigureWebHost runs, seeding (via _factory.Services)
    // and the actual HTTP server (via CreateClient()) could end up pointing
    // at two different in-memory databases. Fixing the name here guarantees
    // both always resolve to the same database.
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}