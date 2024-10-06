using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QBic.Authentication;
using QBic.Core.Auth;
using QBic.Core.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Middleware
{
    public class ContextMiddleware
    {
        private readonly RequestDelegate Next;
        private ILogger Logger { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }
        private readonly DataStore DataStore;
        private readonly IAuthResolver AuthResolver;

        private readonly ConcurrentDictionary<string, IUser> UserCache = new ConcurrentDictionary<string, IUser>();
        private readonly ConcurrentDictionary<string, List<string>> UserRolesCache = new ConcurrentDictionary<string, List<string>>();

        public ContextMiddleware(RequestDelegate next, ILogger<ContextMiddleware> logger, IHttpContextAccessor httpContextAccessor, DataStore dataStore, IAuthResolver authResolver)
        {
            Logger = logger;

            Next = next;
            HttpContextAccessor = httpContextAccessor;
            DataStore = dataStore;
            AuthResolver = authResolver;
        }

        private async Task<IUser> GetUser(string userId)
        {
            if (UserCache.TryGetValue(userId, out IUser cachedValue))
            {
                return cachedValue;
            }

            var userName = HttpContextAccessor.HttpContext.User.Identity.Name;
            if (HttpContextAccessor.HttpContext.User != null)
            {
                var result = await AuthResolver.GetUser(HttpContextAccessor.HttpContext.User);

                UserCache.TryAdd(HttpContextAccessor.HttpContext.User.Identity.Name, result);
                return result;
            }
            return null;
        }

        private async Task<List<string>> GetUserRoles(IUser user)
        {
            if (UserRolesCache.TryGetValue(user.Id, out List<string> cachedValue))
            {
                return cachedValue;
            }

            using var session = DataStore.OpenStatelessSession();

            UserRole roleAlias = null;
            var roles = session.QueryOver<UserRoleAssociation>()
                                   .Left.JoinAlias(x => x.UserRole, () => roleAlias)
                                   .Where(x => x.User.Id == user.Id)
                                   .Select(x => roleAlias.Id)
                                   .List<string>()
                                   .ToList();
            UserRolesCache.TryAdd(user.Id, roles);
            return roles;
        }

        public async Task Invoke(HttpContext context)
        {
            Logger.LogInformation("Context middle-ware being executed");

            var userId = HttpContextAccessor.HttpContext.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var currentUser = await GetUser(userId);

                if (currentUser != null)
                {
                    HttpContextAccessor.HttpContext.Items.Add(QbicConstants.USER_CONTEXT_FIELD_NAME, currentUser);

                    var roles = await GetUserRoles(currentUser);
                    HttpContextAccessor.HttpContext.Items.Add(QbicConstants.USER_ROLES_CONTEXT_FIELD_NAME, roles);
                }
                else
                {
                    HttpContextAccessor.HttpContext.Items.Add(QbicConstants.USER_CONTEXT_FIELD_NAME, null);
                    HttpContextAccessor.HttpContext.Items.Add(QbicConstants.USER_ROLES_CONTEXT_FIELD_NAME, new List<string>());
                }
            }

            await Next(context);
        }
    }
}
