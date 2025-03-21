using System.Security.Claims;

namespace UrlShortenerPro.Api.Extensions
{
    /// <summary>
    /// Расширения для класса ClaimsPrincipal
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Возвращает идентификатор пользователя из клеймов
        /// </summary>
        /// <param name="user">Экземпляр ClaimsPrincipal</param>
        /// <returns>ID пользователя или 0, если клейм отсутствует или невалиден</returns>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return 0;
            }
            
            return userId;
        }
    }
} 