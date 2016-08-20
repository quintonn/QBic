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
        protected static DataStore Store { get; set; }
        protected static IUnityContainer Container { get; set; }
        protected static EventService EventService { get; set; }

        private static bool SetupDone = false;

        public CoreProcessor(IUnityContainer container)
        {
            if (SetupDone == false)
            {
                Container = container;

                ApplicationSettings = container.Resolve<IApplicationSettings>();
                Store = container.Resolve<DataStore>();
                EventService = container.Resolve<EventService>();
            
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
            using (var session = Store.OpenSession())
            {
                ApplicationSettings.SetupDefaults(session);
                session.Flush();
            }
        }

        internal async Task<IHttpActionResult> Process(int eventId, HttpRequestMessage requestMessage)
        {
            try {
                var result = await ProcessEvent(eventId);
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

        protected List<int> GetAllowedEventsForUser(ISession session, string userId)
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
}