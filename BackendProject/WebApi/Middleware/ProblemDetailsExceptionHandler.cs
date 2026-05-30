using AppCore.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebApi.Middleware;

public class ProblemDetailsExceptionHandler(
    ProblemDetailsFactory factory, ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode = exception switch
        {
            GateNotFoundException => StatusCodes.Status404NotFound,
            InvalidGateOperationException => StatusCodes.Status400BadRequest, 
            InvalidCredentialsException => StatusCodes.Status401Unauthorized,
            TokenException => StatusCodes.Status401Unauthorized,
            UserStatusException => StatusCodes.Status403Forbidden,
            _ => 0
        };

        if (statusCode != 0)
        {
            logger.LogInformation($"Obsłużono wyjątek: {exception.Message}");
            
            var problem = factory.CreateProblemDetails(
                context,
                statusCode,
                title: "Błąd serwisu",
                detail: exception.Message
            );

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true; 
        }
        
        return false; 
    }
}