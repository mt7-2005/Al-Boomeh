using Al_BoomehServices;
using System.Net;
using System.Text.Json;

namespace Al_BoomehAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex,
                "Business rule violation on {Method} {Path}: {Message}",
                context.Request.Method, context.Request.Path, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var body = JsonSerializer.Serialize(new
            {
                error = ex.Message
            });

            await context.Response.WriteAsync(body);
        }
        catch (Exception ex)
        {
            var correlationId = Guid.NewGuid().ToString();

            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}. CorrelationId: {CorrelationId}",
                context.Request.Method, context.Request.Path, correlationId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var body = JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred. Please contact support with the reference below.",
                correlationId
            });

            await context.Response.WriteAsync(body);
        }
    }
}