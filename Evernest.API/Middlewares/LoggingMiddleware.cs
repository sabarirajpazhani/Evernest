using System.Diagnostics;

namespace Evernest.API.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            _logger.LogInformation("HTTP {Method} {Path} started", request.Method, request.Path);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var response = context.Response;
                
                var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
                _logger.Log(logLevel, 
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                    request.Method, request.Path, response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
