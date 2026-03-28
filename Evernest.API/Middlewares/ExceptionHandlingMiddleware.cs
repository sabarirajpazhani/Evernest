using System.Net;
using System.Text.Json;

namespace Evernest.API.Middlewares
{
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                KeyNotFoundException => CreateErrorResponse(context, HttpStatusCode.NotFound, "Resource not found", exception.Message),
                UnauthorizedAccessException => CreateErrorResponse(context, HttpStatusCode.Unauthorized, "Unauthorized", exception.Message),
                InvalidOperationException => CreateErrorResponse(context, HttpStatusCode.BadRequest, "Invalid operation", exception.Message),
                ArgumentException => CreateErrorResponse(context, HttpStatusCode.BadRequest, "Invalid argument", exception.Message),
                _ => CreateErrorResponse(context, HttpStatusCode.InternalServerError, "An error occurred", "An unexpected error occurred. Please try again later.")
            };

            return context.Response.WriteAsync(response);
        }

        private static string CreateErrorResponse(HttpContext context, HttpStatusCode statusCode, string error, string message)
        {
            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new
            {
                StatusCode = statusCode,
                Error = error,
                Message = message,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(errorResponse, options);
        }
    }
}
