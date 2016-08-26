using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Controllers;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class CoreProcessor<T>
    {
        protected static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = "dd-MM-yyyy" };
        protected static IApplicationSettings ApplicationSettings { get; set; }
        protected static IUnityContainer Container { get; set; }
        protected static EventService EventService { get; set; }
        protected static DataService DataService { get; set; }
        protected static AuditService AuditService { get; set; }

        private static bool SetupDone = false;

        public CoreProcessor(IUnityContainer container)
        {
            if (SetupDone == false)
            {
                Container = container;

                ApplicationSettings = container.Resolve<IApplicationSettings>();
                EventService = container.Resolve<EventService>();
                DataService = container.Resolve<DataService>();
                AuditService = container.Resolve<AuditService>();
            
                PopulateDefaultValues();
                SetupDone = true;
            }
        }

        public IDictionary<int, IEvent> EventList
        {
            get
            {
                return EventService.EventList;
            }
        }

        private static void PopulateDefaultValues()
        {
            ApplicationSettings.SetupDefaults();
        }

        internal async Task<IHttpActionResult> Process(int eventId, HttpRequestMessage requestMessage)
        {
            try
            {
                await AuditService.LogUserEvent(eventId);
                var result = await ProcessEvent(eventId);

                if (result is FileActionResult)
                {
                    return result as FileActionResult;
                }

                var jsonResult = new JsonResult<T>(result, JSON_SETTINGS, Encoding.UTF8, requestMessage);

                return jsonResult;
            }
            catch (Exception error)
            {
                return new BadRequestErrorMessageResult(error.Message, new DefaultContentNegotiator(), requestMessage, new List<MediaTypeFormatter>());
            }
        }
        public abstract Task<T> ProcessEvent(int eventId);
        protected string GetRequestData()
        {
            using (var stream = HttpContext.Current.Request.InputStream)
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                var res = System.Text.Encoding.UTF8.GetString(mem.ToArray());
                return res;
            }
        }

        protected List<int> GetAllowedEventsForUser(string userId)
        {
            using (var session = DataService.OpenSession())
            {
                var roles = session.CreateCriteria<UserRoleAssociation>()
                                       .CreateAlias("User", "user")
                                       .Add(Restrictions.Eq("user.Id", userId))
                                       .List<UserRoleAssociation>()
                                       .ToList();

                var userRoles = roles.Select(r => r.UserRole.Id).ToArray();
                var eventRoleAssociations = session.CreateCriteria<EventRoleAssociation>()
                                                   .CreateAlias("UserRole", "role")
                                                   .Add(Restrictions.In("role.Id", userRoles))
                                                   .List<EventRoleAssociation>();

                var events = eventRoleAssociations.Select(e => e.Event).ToList();
                return events;
            }
        }

        protected async Task<User> GetLoggedInUser()
        {
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync() as User;
            return user;
        }
    }
}