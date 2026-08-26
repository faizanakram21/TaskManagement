using FluentValidation;
using System.Net;
using System.Text.Json;

namespace TaskManagement.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            await HandleValidationException(context, ex);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Not found: {Message}", ex.Message);
            await HandleException(context, ex.Message, HttpStatusCode.NotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized: {Message}", ex.Message);
            await HandleException(context, ex.Message, HttpStatusCode.Forbidden);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred");
            await HandleException(context, "Server error!", HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleValidationException(
        HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
        var response = new { errors };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task HandleException(
        HttpContext context, string message, HttpStatusCode statusCode)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { errors = new[] { message } };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}