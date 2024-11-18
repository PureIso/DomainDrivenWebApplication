using Serilog.Context;

namespace DomainDrivenWebApplication.API.Middleware;

/// <summary>
/// Middleware for handling correlation IDs in HTTP requests and responses.
/// Generates a new correlation ID if one is not provided in the request headers and ensures it is passed along in the response headers.
/// Also adds the correlation ID to the log context for tracking across requests.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the incoming HTTP request, retrieves or generates a correlation ID, and ensures it is included in the response headers.
    /// The correlation ID is also pushed to the log context for tracking throughout the request lifecycle.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        string? correlationId = context.Request.Headers.ContainsKey(CorrelationIdHeader)
            ? context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            : Guid.NewGuid().ToString();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Store the correlation ID in the log context
        using (LogContext.PushProperty(CorrelationIdHeader, correlationId))
        {
            await _next(context);
        }
    }
}