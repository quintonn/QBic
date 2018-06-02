using System;
using System.Linq;
using System.Web.Http;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Controllers
{
    public class ConditionalAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var eventIdString = actionContext.Request.RequestUri.Segments.Last();
            var eventId = Convert.ToInt32(eventIdString);

            var iEvent = EventService.EventList[eventId];
            if (iEvent.RequiresAuthorization)
            {
                base.OnAuthorization(actionContext);
            }
            //else
            //{
            //    //XXXUtils.SetCurrentUser("System");
            //}
        }
    }
}