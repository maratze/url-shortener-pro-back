using System.IdentityModel.Tokens.Jwt;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Middleware;

public class JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                // Проверка токена для отладки
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    logger.LogDebug("JWT Token Header: {Header}", jwtToken.Header);
                    logger.LogDebug("JWT Token Payload: {Claims}",
                        string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}: {c.Value}")));
                    logger.LogDebug("JWT Token Valid From: {ValidFrom}, Valid To: {ValidTo}", jwtToken.ValidFrom,
                        jwtToken.ValidTo);
                }
                else
                {
                    logger.LogWarning("Invalid JWT token format");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing JWT token");
            }
        }
        else
        {
            logger.LogDebug("No Authorization header found");
        }

        await next(context);
    }
}