using Newtonsoft.Json;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.Services.Background;
using WebsiteTemplate.Data;
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

        protected static UserContext UserContext { get; set; }

        private static bool SetupDone = false;

        private static object LockObject = new object();

        public CoreProcessorBase(IUnityContainer container)
        {
            lock(LockObject)
            {
                if (SetupDone == false)
                {
                    Container = container;

                    //TODO: maybe all of thse should go outside of the "SetupDone" check and run everytime.
                    ApplicationStartup = container.Resolve<ApplicationStartup>();
                    EventService = container.Resolve<EventService>();
                    DataService = container.Resolve<DataService>();
                    AuditService = container.Resolve<AuditService>();
                    BackgroundService = container.Resolve<BackgroundService>();
                    UserContext = container.Resolve<UserContext>();

                    PopulateDefaultValues();
                    SetupDone = true;
                    BackgroundService.StartBackgroundJobs();
                }
            }
            using (var session = DataService.OpenStatelessSession())
            //using (var transaction = session.BeginTransaction())
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
            return XXXUtils.GetCurrentRequestData();
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
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync(UserContext) as User;
            return user;
        }
    }
}