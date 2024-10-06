using Microsoft.AspNetCore.Identity;
using QBic.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QBic.Core.Auth
{
    public class QbicAuthResolver : IAuthResolver
    {
        private readonly UserManager<IUser> UserManager;

        public QbicAuthResolver(UserManager<IUser> userManager)
        {
            UserManager = userManager;
        }

        public async Task<IUser> GetUser(ClaimsPrincipal principal)
        {
            if (!string.IsNullOrWhiteSpace(principal?.Identity?.Name))
            {
                var user = await UserManager.FindByNameAsync(principal.Identity.Name);
                return user;
            }
            return null;
        }
    }
}
