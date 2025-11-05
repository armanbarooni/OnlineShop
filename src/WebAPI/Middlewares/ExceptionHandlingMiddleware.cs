using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Common.Models;
using OnlineShop.Application.Exceptions;
using ValidationException = FluentValidation.ValidationException;

namespace OnlineShop.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
        {

            logger.LogError(ex, "Unhandled exception occurred.");

            int statusCode;
            string message;


            switch (ex)
            {
                case NotFoundException nf:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = nf.Message;
                    break;

                case ValidationException ve:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = string.Join("; ", ve.Errors.Select(e => e.ErrorMessage));
                    break;

                case UnauthorizedAccessException ua:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = ua.Message;
                    break;

                case ArgumentException ae:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = ae.Message;
                    break;

                case InvalidOperationException ioe:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = ioe.Message;
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = "خطای غیرمنتظره‌ای رخ داده است.";
                    break;
            }

            var result = Result<string>.Failure(message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
