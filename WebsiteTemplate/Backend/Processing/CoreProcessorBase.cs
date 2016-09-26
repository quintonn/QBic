using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class CoreProcessorBase
    {
        protected static JsonSerializerSettings JSON_SETTINGS;
        protected static ApplicationStartup ApplicationStartup { get; set; }
        protected static IUnityContainer Container { get; set; }
        protected static EventService EventService { get; set; }
        protected static DataService DataService { get; set; }
        protected static AuditService AuditService { get; set; }
        protected static BackgroundService BackgroundService { get; set; }

        private static bool SetupDone = false;

        public CoreProcessorBase(IUnityContainer container)
        {
            if (SetupDone == false)
            {
                Container = container;

                ApplicationStartup = container.Resolve<ApplicationStartup>();
                EventService = container.Resolve<EventService>();
                DataService = container.Resolve<DataService>();
                AuditService = container.Resolve<AuditService>();
                BackgroundService = container.Resolve<BackgroundService>();

                PopulateDefaultValues();
                SetupDone = true;
                BackgroundService.StartBackgroundJobs();
            }

            using (var session = DataService.OpenSession())
            {
                var appSettings = session.QueryOver<Models.SystemSettings>().List<Models.SystemSettings>().FirstOrDefault();
                if (appSettings != null)
                {
                    JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = appSettings.DateFormat };
                }
                else
                {
                    JSON_SETTINGS = new JsonSerializerSettings { DateFormatString = "yyyy-mm-dd" };
                }
            }
        }

        private static void PopulateDefaultValues()
        {
            CreateInternalUsers();
            ApplicationStartup.SetupDefaultsInternal();
        }

        private static void CreateInternalUsers()
        {
            using (var session = DataService.OpenSession())
            {
                var exists = session.QueryOver<User>().Where(i => i.UserName == "System").RowCount() > 0;
                if (exists == false)
                {
                    var systemUser = new User()
                    {
                        CanDelete = false,
                        Email = Container.Resolve<ApplicationSettingsCore>().SystemEmailAddress,
                        EmailConfirmed = true,
                        UserName = "System",
                        UserStatus = UserStatus.Active,
                        PasswordHash = ""
                    };

                    session.Save(systemUser);
                    session.Flush();
                }
            }
        }

        public IDictionary<int, IEvent> EventList
        {
            get
            {
                return EventService.EventList;
            }
        }

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