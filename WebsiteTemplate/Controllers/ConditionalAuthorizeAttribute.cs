using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Controllers
{
    //TODO: This is not working correctly anymore. It just authorizes everything
    public class ConditionalAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        //public void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        //{
        //    var eventIdString = actionContext.Request.RequestUri.Segments.Last();
        //    var eventId = Convert.ToInt32(eventIdString);

        //    var iEvent = EventService.EventMenuList[eventId];
        //    if (iEvent.RequiresAuthorization)
        //    {
        //        base.OnAuthorization(actionContext);
        //    }
        //    //else
        //    //{
        //    //    //XXXUtils.SetCurrentUser("System");
        //    //}
        //}
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var eventIdString = context.HttpContext.Request.Path.Value.Split("/").Last();
            var eventId = Convert.ToInt32(eventIdString);

            var iEvent = EventService.EventMenuList[eventId];
            if (iEvent.RequiresAuthorization)
            {
                Console.WriteLine("X");
                //base.OnAuthorization(actionContext);
            }
            //else
            //{
            //    //XXXUtils.SetCurrentUser("System");
            //}
        }
    }
}