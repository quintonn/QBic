using NHibernate.Criterion;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Controllers
{
    public class RoleAuthorization : AuthorizationFilterAttribute
    {
        private List<string> Roles { get; set; }

        public RoleAuthorization(params string[] roles)
        {
            Roles = roles.ToList();
        }

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (Roles == null || Roles.Count == 0)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "No roles set for RoleAuthorozation"
                };
                return;
            }

            var identity = actionContext.RequestContext.Principal.Identity as System.Security.Claims.ClaimsIdentity;

            User user;
            var store = DataStore.GetInstance();
            using (var session = store.OpenSession())
            {
                user = session.CreateCriteria<User>()
                              .Add(Restrictions.Eq("UserName", identity.Name))
                              .UniqueResult<User>();
                if (user == null)
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Invalid user (unknown user)"
                    };
                    return;
                }
            }

            //if (!Roles.Contains(user.UserRole.Name))
            //{
            //    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
            //    {
            //        ReasonPhrase = "Unauthorized to perform this action"
            //    };
            //}

            base.OnAuthorization(actionContext);
        }
    }
}