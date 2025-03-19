namespace UrlShortenerPro.Api.Middleware
{
    /// <summary>
    /// Middleware для детального логирования запросов и ответов API 
    /// </summary>
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Логируем информацию о входящем запросе
            logger.LogInformation(
                "Request {Method} {Scheme}://{Host}{Path}{QueryString} from {IP} with {ContentType}",
                context.Request.Method,
                context.Request.Scheme,
                context.Request.Host,
                context.Request.Path,
                context.Request.QueryString,
                context.Connection.RemoteIpAddress,
                context.Request.ContentType);

            try
            {
                // Вызываем следующий middleware в цепочке
                await next(context);

                // Логируем ответ
                logger.LogInformation(
                    "Response {StatusCode} for {Method} {Path}",
                    context.Response.StatusCode,
                    context.Request.Method,
                    context.Request.Path);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, 
                    "Error processing request {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                throw; // Перебрасываем исключение, чтобы его мог обработать другой middleware
            }
        }
    }
} 