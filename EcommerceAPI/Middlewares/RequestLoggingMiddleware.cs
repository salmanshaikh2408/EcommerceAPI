using System.Diagnostics;

namespace EcommerceAPI.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Incoming request: {method} {url}", context.Request.Method, context.Request.Path);
            
            await _next(context);

            stopwatch.Stop();
            _logger.LogInformation("Outgoing response: {statusCode} completed in {Elapsed}ms", context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
