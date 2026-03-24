using EventCalendarAPI.DTOs.Response;
using EventCalendarAPI.Exceptions;
using System.Text.Json;

namespace EventCalendarAPI.Helpers
{
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            Console.WriteLine("EXCEPTION: " + exception.Message);
            Console.WriteLine("STACK: " + exception.StackTrace);

            var (statusCode, message, errors) = exception switch
            {
                EntityNotFoundException => (StatusCodes.Status404NotFound, exception.Message, (List<string>?)null),
                DuplicateEntityException => (StatusCodes.Status409Conflict, exception.Message, null),
                UnauthorizedException => (StatusCodes.Status403Forbidden, exception.Message, null),
                Exceptions.ValidationException ve => (StatusCodes.Status400BadRequest, exception.Message, ve.Errors),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", null),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
            };

            context.Response.StatusCode = statusCode;

            var response = ApiResponseDto<object>.Fail(message, errors);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
