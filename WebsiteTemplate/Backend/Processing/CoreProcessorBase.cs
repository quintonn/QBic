using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Criterion;
using QBic.Authentication;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.Services.Background;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Processing
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private string DateFormat { get; set; }

        public DateTimeConverter(string dateFormat)
        {

        }
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            //writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
            writer.WriteStringValue(value.ToUniversalTime().ToString(DateFormat));
        }
    }
    public abstract class CoreProcessorBase
    {
        protected static JsonSerializerOptions JSON_SETTINGS;
        protected static ApplicationStartup ApplicationStartup { get; set; }
        protected static IServiceProvider Container { get; set; }
        protected static EventService EventService { get; set; }
        protected static DataService DataService { get; set; }
        protected static AuditService AuditService { get; set; }
        protected static BackgroundService BackgroundService { get; set; }

        protected static UserManager<IUser> UserManager { get; set; }

        private static bool SetupDone = false;

        private static object LockObject = new object();

        public CoreProcessorBase(IServiceProvider container)
        {
            try
            {
                lock (LockObject)
                {
                    Container = container;

                    UserManager = Container.GetService<UserManager<IUser>>();

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

                    JSON_SETTINGS = new JsonSerializerOptions
                    {
                        //DateFormatString = WebsiteUtils.DateFormat 
                    };
                    JSON_SETTINGS.Converters.Add(new DateTimeConverter(WebsiteUtils.DateFormat));
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

        //public IDictionary<int, Type> EventList
        //{
        //    get
        //    {
        //        return EventService.EventList;
        //    }
        //}

        protected async Task<string> GetRequestData()
        {
            return await WebsiteUtils.GetCurrentRequestData(Container.GetService<IHttpContextAccessor>());
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
            var user = await QBicUtils.GetLoggedInUserAsync(UserManager, Container.GetService<IHttpContextAccessor>()) as User;
            return user;
        }

        
    }
}