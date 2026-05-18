using Microsoft.AspNetCore.Mvc;

namespace NTech.KeyVault.Api.Middlewares
{
    public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            // The middleware catches any unhandled exceptions that occur during the processing of the request pipeline.
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // The middleware maps specific exception types to appropriate HTTP status codes and constructs a standardized error response using the Problem Details format.
            var status = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                NotSupportedException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            // The middleware logs the exception details for monitoring and debugging purposes.
            var problem = new ProblemDetails
            {
                Status = status,
                Title = status switch
                {
                    400 => "Bad request",
                    401 => "Unauthorized",
                    404 => "Resource not found",
                    _ => "Internal server error"
                },
                Detail = status == 500
                    ? "An unexpected error occurred."
                    : ex.Message,
                Instance = context.Request.Path,
                Type = status switch
                {
                    400 => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
                    401 => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/401",
                    404 => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/404",
                    _ => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500"
                }
            };

            problem.Extensions["timestamp"] = DateTime.UtcNow;

            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            return context.Response.WriteAsJsonAsync(problem);
        }
    }
}
