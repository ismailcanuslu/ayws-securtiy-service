using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Middleware;

public class GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, errors) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest,
                ve.Errors.Select(e => e.ErrorMessage).ToList()),
            KeyNotFoundException => (StatusCodes.Status404NotFound,
                new List<string> { exception.Message }),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,
                new List<string> { "Yetkisiz erişim." }),
            _ => (StatusCodes.Status500InternalServerError,
                new List<string> { "Beklenmeyen bir hata oluştu." })
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = "Hata",
                Extensions = { ["errors"] = errors }
            }, cancellationToken);

        return true;
    }
}
