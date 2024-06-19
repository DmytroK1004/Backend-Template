using Newtonsoft.Json;
using Presentation.Exceptions;
using System.Net;

namespace Presentation.Middleware
{
    public class CustomErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IBestPracticesException> _logger;
        public CustomErrorMiddleware(RequestDelegate next, ILogger<IBestPracticesException> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // EVERY CALL CODE
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // PROCESS PRE ERROR
                await HandleExceptionAsync(httpContext, ex);
                // PROCESS POST ERROR
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            if (ex is IBestPracticesException exception)
            {
                if (exception.HttpStatusCode.HasValue) context.Response.StatusCode = (int)exception.HttpStatusCode.Value;
                await context.Response.WriteAsync(exception.ToJson());
                _logger?.LogError(exception.EventId, ex, exception.Message);
            }
            else
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Something went worng"
                }));
                _logger?.LogError(new EventId(0, "UnknownError"), ex, ex.Message);
            }
        }
    }
}
