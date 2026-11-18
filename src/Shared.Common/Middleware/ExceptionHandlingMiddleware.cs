using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Exceptions;

namespace Shared.Common.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning("Application error ({ErrorCode}): {Message}", ex.ErrorCode, ex.Message);
            await WriteErrorResponseAsync(context, ex.StatusCode, ex.ErrorCode, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "An internal server error occurred.",
                Array.Empty<string>());
        }
    }

    private static Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string code,
        string message,
        IReadOnlyCollection<string> details)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error = new
            {
                code,
                message,
                details
            }
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}