using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(RequestDelegate next,
                                   ILogger<ExceptionMiddleware> logger,
                                   IHostEnvironment environment)
        {
            this._next = next;
            this._logger = logger;
            this._environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                var statusCode = (int)HttpStatusCode.InternalServerError;

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = statusCode;

                var response = _environment.IsDevelopment() ?
                        getDeveloperException(ex, statusCode) :
                        getUserException(statusCode);
                        
                var options = new JsonSerializerOptions
                                {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

                var json = JsonSerializer
                                .Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }

        private static AppException getUserException(int statusCode)
        {
            return new AppException(statusCode, "Server Error");
        }

        private static AppException getDeveloperException(Exception ex, int statusCode)
        {
            return new AppException(statusCode,
                                                     ex.Message,
                                                     ex.StackTrace?.ToString());
        }
    }
}