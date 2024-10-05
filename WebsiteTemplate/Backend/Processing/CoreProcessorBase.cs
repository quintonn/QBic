using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NHibernate.Criterion;
using QBic.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.Services.Background;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public abstract class CoreProcessorBase
    {
        protected static JsonSerializerSettings JSON_SETTINGS;
        protected static ApplicationStartup ApplicationStartup { get; set; }
        protected static IServiceProvider Container { get; set; }
        protected static EventService EventService { get; set; }
        protected static DataService DataService { get; set; }
        protected static AuditService AuditService { get; set; }
        protected static BackgroundService BackgroundService { get; set; }

        private static bool SetupDone = false;

        private static object LockObject = new object();

        private readonly ContextService ContextService;
        public CoreProcessorBase(IServiceProvider container)
        {
            try
            {
                lock (LockObject)
                {
                    Container = container;

                    ContextService = Container.GetService<ContextService>();

                    if (SetupDone == false)
                    {
                        ApplicationStartup = container.GetService<ApplicationStartup>();
                        EventService = container.GetService<EventService>();
                        DataService = container.GetService<DataService>();
                        AuditService = container.GetService<AuditService>();
                        BackgroundService = container.GetService<BackgroundService>();


                        PopulateDefaultValues();
                        SetupDone = true;
                        BackgroundService.StartBackgroundJobs();
                    }

                    JSON_SETTINGS = new JsonSerializerSettings
                    {
                        // hardcode for now because the front-end and back-end don't match
                        DateFormatString = "dd-MMM-yyyy",// WebsiteUtils.DateFormat,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                }

            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
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
                        Email = Container.GetService<ApplicationSettingsCore>().SystemEmailAddress,
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

        protected async Task<string> GetRequestData()
        {
            return await WebsiteUtils.GetCurrentRequestData(Container.GetService<IHttpContextAccessor>());
        }

        protected List<int> GetAllowedEventsForUser(string userId)
        {
            using (var session = DataService.OpenSession())
            {
                var userRoles = ContextService.GetRequestUserRoles().ToArray();
                var eventRoleAssociations = session.CreateCriteria<EventRoleAssociation>()
                                                   .CreateAlias("UserRole", "role")
                                                   .Add(Restrictions.In("role.Id", userRoles))
                                                   .List<EventRoleAssociation>();

                var events = eventRoleAssociations.Select(e => e.Event).ToList();
                return events;
            }
        }

        protected async Task<IUser> GetLoggedInUser()
        {
            var user = ContextService.GetRequestUser();
            return user;
        }

        
    }
}