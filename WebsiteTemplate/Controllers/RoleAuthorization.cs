using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NHibernate.Criterion;
using QBic.Core.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Controllers
{
    public class RoleAuthorization : IAuthorizationFilter
    {
        private List<string> Roles { get; set; }

        public RoleAuthorization(params string[] roles)
        {
            Roles = roles.ToList();
        }

        public void OnAuthorization(AuthorizationFilterContext actionContext)
        {
            if (Roles == null || Roles.Count == 0)
            {
                //actionContext.Result = new  BadRequestObjectResult.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                //{
                //    ReasonPhrase = "No roles set for RoleAuthorozation"
                //};
                actionContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                actionContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "No roles set for RoleAuthorozation";
                return;
            }

            var identity = actionContext.HttpContext.User.Identity;//.RequestContext.Principal.Identity as System.Security.Claims.ClaimsIdentity;

            User user;
            var store = DataStore.GetInstance(false, null);
            using (var session = store.OpenSession())
            {
                user = session.CreateCriteria<User>()
                              .Add(Restrictions.Eq("UserName", identity.Name))
                              .UniqueResult<User>();
                if (user == null)
                {
                    //actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                    //{
                    //    ReasonPhrase = "Invalid user (unknown user)"
                    //};
                    actionContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                    actionContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Invalid user (unknown user)";
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

            //base.OnAuthorization(actionContext);
        }
    }
}