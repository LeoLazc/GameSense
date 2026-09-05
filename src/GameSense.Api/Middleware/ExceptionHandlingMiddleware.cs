using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentValidation;
using GameSense.Core.Exceptions;

namespace GameSense.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing request {Method} {Path}", context.Request.Method, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/problem+json";

            var pd = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Detail = exception.Message,
                Status = StatusCodes.Status500InternalServerError
            };

            switch (exception)
            {
                case AiResponseParseException:
                    pd.Title = "AI response parsing failed";
                    pd.Status = StatusCodes.Status422UnprocessableEntity;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                case ValidationException vex:
                    pd.Title = "Validation failed";
                    pd.Status = StatusCodes.Status400BadRequest;
                    pd.Detail = "One or more validation errors occurred.";
                    // include errors in extensions
                    pd.Extensions["errors"] = vex.Errors;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                case QuizConflictException qce:
                    pd.Title = "Quiz operation conflict";
                    pd.Status = StatusCodes.Status409Conflict;
                    pd.Detail = qce.Message;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                case ExpertiseEligibilityException:
                    pd.Title = "Review not authorized";
                    pd.Status = StatusCodes.Status403Forbidden;
                    pd.Detail = exception.Message;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                case PredictionConflictException:
                    pd.Title = "GOTY prediction conflict";
                    pd.Status = StatusCodes.Status409Conflict;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                case ReviewConflictException:
                    pd.Title = "Review conflict";
                    pd.Status = StatusCodes.Status409Conflict;
                    pd.Detail = exception.Message;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                case KeyNotFoundException:
                    pd.Title = "Resource not found";
                    pd.Status = StatusCodes.Status404NotFound;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
                default:
                    pd.Status = StatusCodes.Status500InternalServerError;
                    context.Response.StatusCode = pd.Status.Value;
                    break;
            }

            if (_env.IsDevelopment())
            {
                pd.Extensions["stackTrace"] = exception.StackTrace;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(pd, options);
            await context.Response.WriteAsync(json);
        }
    }
}
