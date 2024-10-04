using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot get userId from token!!!"));

            return userId;      
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            var username = user.FindFirstValue(ClaimTypes.Name);

            if(username == null) throw new Exception("Cannot get username from token!!!");

            return username;
        }
    }
}