using Azure;
using EcommerceAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EcommerceAPI.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            //1. Log the exception
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            //3. Write the error response
            var response = new ProblemDetails
            {
                Title = "An error occurred",
                Status = (int)System.Net.HttpStatusCode.InternalServerError,
                Detail = exception.Message, // You might want to remove this in production for security reasons
                Instance = httpContext.Request.Path
            };

            switch(exception)
            {
                case BadRequestException badRequest:
                    response.Title = "Bad Request";
                    response.Status = (int)HttpStatusCode.BadRequest;
                    response.Detail = badRequest.Message;
                    break;
                case NotFoundException NotFound:
                    response.Title = "Not Found";
                    response.Status = (int)HttpStatusCode.NotFound;
                    response.Detail = NotFound.Message;
                    break;
                default:
                    response.Title = "Internal Server Error";
                    response.Status = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            //2. Set the response status code and content type
            httpContext.Response.StatusCode = response.Status.Value;
            httpContext.Response.ContentType = "application/json";

            // 4. Return JSON response
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }
    }
}
