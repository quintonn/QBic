using QBic.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QBic.Core.Auth
{
    public interface  IAuthResolver
    {
        Task<IUser> GetUser(ClaimsPrincipal principal);
    }
}
