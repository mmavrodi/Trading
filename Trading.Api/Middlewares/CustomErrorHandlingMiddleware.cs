using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Trading.Api.Middlewares
{
    public class CustomErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public CustomErrorHandlingMiddleware(RequestDelegate next, ILogger<CustomErrorHandlingMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "FluentValidation failure. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleFluentValidationAsync(context, vex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing request. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleFluentValidationAsync(HttpContext context, ValidationException vex)
        {
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var vpd = new ValidationProblemDetails(errors)
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };
            vpd.Extensions["traceId"] = context.TraceIdentifier;
            context.Response.StatusCode = vpd.Status ?? StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            var json = JsonSerializer.Serialize(vpd, JsonOptions);
            return context.Response.WriteAsync(json);
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (status, title) = exception switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden"),
                ArgumentException or ArgumentNullException or InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad request"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };

            var pd = new ProblemDetails
            {
                Type = status == StatusCodes.Status500InternalServerError
                    ? "https://httpstatuses.com/500"
                    : "about:blank",
                Title = title,
                Status = status,
                Instance = context.Request.Path
            };
            pd.Extensions["traceId"] = context.TraceIdentifier;
            if (_env.IsDevelopment())
            {
                pd.Extensions["exceptionMessage"] = exception.Message;
                pd.Extensions["stackTrace"] = exception.StackTrace;
            }

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            var json = JsonSerializer.Serialize(pd, JsonOptions);
            return context.Response.WriteAsync(json);
        }
    }
}

