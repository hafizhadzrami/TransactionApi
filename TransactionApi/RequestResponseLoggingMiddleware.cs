using System.Text;
using log4net;
using TransactionApi.Utilities;

namespace TransactionApi
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ILog _log = LogManager.GetLogger(typeof(RequestResponseLoggingMiddleware));

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log Request
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(body))
                {
                    string sanitized = LogSanitizer.Sanitize(body);
                    _log.Info($"📥 HTTP Request Body: {sanitized}");
                }
            }

            // Copy original response stream
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Log Response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (!string.IsNullOrEmpty(responseText))
            {
                string sanitized = LogSanitizer.Sanitize(responseText);
                _log.Info($"📤 HTTP Response Body: {sanitized}");
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    // Extension untuk mudah tambah middleware
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
