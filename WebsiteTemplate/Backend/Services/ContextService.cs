using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QBic.Authentication;
using QBic.Core.Utilities;
using System.Collections.Generic;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class ContextService
    {
        private static readonly ILogger Logger = SystemLogger.GetLogger<ContextService>();
        private readonly IHttpContextAccessor HttpContextAccessor;

        public ContextService(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns the user making the current request, if an authenticated request was made.
        /// Else returns null
        /// </summary>
        /// <returns>The current user, or null</returns>
        public IUser GetRequestUser()
        {
            if (HttpContextAccessor.HttpContext.Items.ContainsKey(QbicConstants.USER_CONTEXT_FIELD_NAME))
            {
                return HttpContextAccessor.HttpContext.Items[QbicConstants.USER_CONTEXT_FIELD_NAME] as IUser;
            }
            return null;
        }

        public List<string> GetRequestUserRoles()
        {
            if (HttpContextAccessor.HttpContext.Items.ContainsKey(QbicConstants.USER_ROLES_CONTEXT_FIELD_NAME))
            {
                return HttpContextAccessor.HttpContext.Items[QbicConstants.USER_ROLES_CONTEXT_FIELD_NAME] as List<string>;
            }

            return new List<string>();
        }
    }
}
