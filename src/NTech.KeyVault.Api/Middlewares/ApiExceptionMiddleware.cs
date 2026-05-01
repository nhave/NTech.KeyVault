using Microsoft.AspNetCore.Mvc;

namespace NTech.KeyVault.Api.Middlewares
{
    public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
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
            var status = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                NotSupportedException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

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
                Detail = ex.Message,
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
