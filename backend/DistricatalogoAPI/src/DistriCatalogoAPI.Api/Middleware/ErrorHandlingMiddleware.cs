using System.Net;
using System.Text.Json;
using DistriCatalogoAPI.Domain.Exceptions;
using FluentValidation;

namespace DistriCatalogoAPI.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationEx:
                    response.Message = "Error de validación";
                    response.Details = validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case DuplicateEmailException duplicateEmailEx:
                    response.Message = duplicateEmailEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;

                case UserNotFoundException userNotFoundEx:
                    response.Message = userNotFoundEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    response.Message = unauthorizedEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case ArgumentException argumentEx:
                    response.Message = argumentEx.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException invalidOpEx:
                    response.Message = invalidOpEx.Message;
                    context.Response.StatusCode = invalidOpEx.Message.Contains("not found") 
                        ? (int)HttpStatusCode.NotFound 
                        : (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    response.Message = "Ha ocurrido un error interno del servidor";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string>? Details { get; set; }
    }
}