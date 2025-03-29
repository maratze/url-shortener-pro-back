using Microsoft.AspNetCore.Http;
using UrlShortenerPro.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace UrlShortenerPro.Api.Middleware
{
    /// <summary>
    /// Middleware для проверки валидности сессии пользователя перед аутентификацией по JWT токену
    /// </summary>
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService, IUserSessionRepository sessionRepository)
        {
            // Получаем токен из заголовка Authorization
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                // Извлекаем ID пользователя из токена
                var userId = jwtService.GetUserIdFromToken(token);

                if (userId.HasValue)
                {
                    // Проверяем, есть ли активная сессия с данным токеном
                    var session = await sessionRepository.GetSessionByTokenAsync(userId.Value, token);

                    if (session == null || !session.IsActive)
                    {
                        _logger.LogWarning("Попытка доступа с недействительной сессией. UserId: {UserId}, IP: {IP}", 
                            userId, context.Connection.RemoteIpAddress);

                        // Если сессия не найдена или не активна, блокируем запрос
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new { message = "Session expired or invalidated. Please login again." });
                        
                        return;
                    }
                    
                    // Сессия валидна, обновляем время последней активности
                    try
                    {
                        await sessionRepository.UpdateSessionActivityAsync(userId.Value, token);
                    }
                    catch (Exception ex)
                    {
                        // Если не удалось обновить активность, просто логируем ошибку, но разрешаем запрос
                        _logger.LogError(ex, "Не удалось обновить время активности сессии для пользователя {UserId}", userId);
                    }
                }
            }

            // Продолжаем обработку запроса
            await _next(context);
        }
    }
} 