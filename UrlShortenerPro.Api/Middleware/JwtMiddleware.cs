using Microsoft.AspNetCore.Http;

namespace UrlShortenerPro.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // This middleware only logs the Authorization header for debugging purposes
        
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        
        if (!string.IsNullOrEmpty(token))
        {
            // Log the token for debugging
            Console.WriteLine($"JWT Token received: {token}");
        }
        
        await _next(context);
    }
}