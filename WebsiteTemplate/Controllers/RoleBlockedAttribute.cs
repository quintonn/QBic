using BasicAuthentication.ControllerHelpers;
using BasicAuthentication.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebsiteTemplate.Data;

namespace WebsiteTemplate.Controllers
{
    public class RoleBlockedAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            /// 
            /*var user = Methods.GetLoggedInUser(actionContext.RequestContext);
            using (var session = new DataStore().OpenSession())
            {
                // get
            }
            var isInRoleTask = CoreAuthenticationEngine.UserManager.IsInRoleAsync(user.Id, "ROLE");
            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "HTTPS Required"
                };
            }
            else
            {
                base.OnAuthorization(actionContext);
            }*/
        }
    }
}