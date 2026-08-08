using System.Net;
using System.Text.Json;
using PlaylistManagement.Api.DTOs.Common;

namespace PlaylistManagement.Api.Middleware
{
    /// <summary>
    /// Safety net for exceptions that escape the pipeline. Expected
    /// business-rule failures (not found, forbidden, duplicate name, wrong
    /// credentials, ...) never reach here — services report those via
    /// Result/Result&lt;T&gt;, handled directly in the controllers
    /// (ApiControllerBase.FromError). Anything that does land here is either
    /// a genuine bug or a framework-level exception (a bad claim, a DB
    /// failure), so a small set of BCL exception types still get mapped to a
    /// sensible status code; anything unrecognized falls back to 500.
    /// </summary>
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
                _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            var response = new ApiErrorResponse
            {
                Success = false,
                Message = message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
