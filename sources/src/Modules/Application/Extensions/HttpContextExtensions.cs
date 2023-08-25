using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Application.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid GetUserId(this IHttpContextAccessor context)
        {
            return Guid.Parse(context.HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
