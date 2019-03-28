using System;
using System.Net;
using System.Threading.Tasks;
using Btc.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Btc.Api.Infrastructure
{
    public class ErrorWrappingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorWrappingMiddleware> _logger;

        public ErrorWrappingMiddleware(RequestDelegate next, ILogger<ErrorWrappingMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            ServiceResponse<bool> errorResponse = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                await _next.Invoke(context).ConfigureAwait(false);
            }
            catch (ValidationException e)
            {
                _logger.LogError(e, "Validation Error occured");
                errorResponse = ServiceResponse<bool>.New(false);

                foreach (var error in e.Errors)
                {
                    foreach (var message in error.Value)
                    {
                        errorResponse.AddMessage($"{error.Key}: {message}");
                    }
                }

                statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Validation Error occured");
                errorResponse = ServiceResponse<bool>.New(false);
                errorResponse.AddMessage(e.Message);
                statusCode = HttpStatusCode.InternalServerError;
            }

            if (!context.Response.HasStarted && errorResponse != null)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var json = JsonConvert.SerializeObject(errorResponse);

                await context.Response.WriteAsync(json).ConfigureAwait(false);
            }
        }
    }
}