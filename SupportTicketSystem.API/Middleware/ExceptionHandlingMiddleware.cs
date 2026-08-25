using Microsoft.EntityFrameworkCore;
using SupportTicketSystem.Application.Exceptions;
using System.Net;
using System.Text.Json;
namespace SupportTicketSystem.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                BusinessRuleException => (HttpStatusCode.BadRequest, ex.Message),
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Forbidden, "You are not authorized to perform this action."),
                DbUpdateException dbEx when IsForeignKeyViolation(dbEx) =>
                (HttpStatusCode.BadRequest, "Cannot delete this user — they have associated tickets, comments, or time logs. Reassign or remove those first."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")

            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }


    }

    private static bool IsForeignKeyViolation(DbUpdateException ex)
    {
        // SQL Server FK violation error number is 547
        return ex.InnerException?.Message.Contains("REFERENCE constraint") == true
            || ex.InnerException?.Message.Contains("FOREIGN KEY") == true;
    }
}