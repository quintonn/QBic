using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Controllers
{
    public class ConditionalAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var eventIdString = context.HttpContext.Request.Path.Value.Split("/").Last();
            var eventId = Convert.ToInt32(eventIdString);

            var iEvent = EventService.EventMenuList[eventId];
            if (iEvent.RequiresAuthorization && context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                context.HttpContext.User = null;
                context.Result = new UnauthorizedResult();
            }
        }
    }
}