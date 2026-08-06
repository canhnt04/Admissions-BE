using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Exceptions;
using Shared.Common.Responses;
using System.Net;
using System.Text.Json;

namespace Shared.Common.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            bool isControlledException = ex is ValidationException || 
                                         ex is UnauthorizedException || 
                                         ex is ForbiddenException || 
                                         ex is NotFoundException || 
                                         ex is ConflictException || 
                                         ex is BusinessException || 
                                         ex is CustomException;

            if (isControlledException)
            {
                _logger.LogWarning("Controlled exception: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
            }
            else
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            }
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Timestamp = Shared.Common.Helpers.TimeHelper.VietnamNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = validationEx.Error.Code;
                response.ErrorKey = validationEx.Error.Key;
                response.Message = validationEx.Error.Message;
                response.ValidationErrors = validationEx.ValidationErrors;
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.ErrorCode = unauthorizedEx.Error.Code;
                response.ErrorKey = unauthorizedEx.Error.Key;
                response.Message = unauthorizedEx.Error.Message;
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.ErrorCode = forbiddenEx.Error.Code;
                response.ErrorKey = forbiddenEx.Error.Key;
                response.Message = forbiddenEx.Error.Message;
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.ErrorCode = notFoundEx.Error.Code;
                response.ErrorKey = notFoundEx.Error.Key;
                response.Message = notFoundEx.Error.Message;
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.ErrorCode = conflictEx.Error.Code;
                response.ErrorKey = conflictEx.Error.Key;
                response.Message = conflictEx.Error.Message;
                break;

            case BusinessException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                response.ErrorCode = businessEx.Error.Code;
                response.ErrorKey = businessEx.Error.Key;
                response.Message = businessEx.Error.Message;
                break;

            case CustomException customEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.ErrorCode = customEx.Error.Code;
                response.ErrorKey = customEx.Error.Key;
                response.Message = customEx.Error.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.ErrorCode = 0;
                response.ErrorKey = "System.UnknownError";
                response.Message = "An unexpected error occurred.";
                break;
        }

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(result);
    }
}
