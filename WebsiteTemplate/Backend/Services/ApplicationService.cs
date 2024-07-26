﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QBic.Core.Utilities;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class ApplicationService
    {
        private static readonly ILogger Logger = SystemLogger.GetLogger<ApplicationService>();
        private static ApplicationSettingsCore ApplicationSettings { get; set; }
        private UserManager<User> UserContext { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }
        private readonly DataService DataService;
        public ApplicationService(ApplicationSettingsCore applicationSettings, UserManager<User> userContext, IHttpContextAccessor httpContextAccessor, DataService dataService)
        {
            ApplicationSettings = applicationSettings;
            UserContext = userContext;
            HttpContextAccessor = httpContextAccessor;
            DataService = dataService;
        }

        
        public object InitializeApplication(string constructorError)
        {
            var version = ApplicationSettings.GetType().Assembly.GetName().Version.ToString();

            using var session = DataService.OpenSession();
            var systemSettings = session.QueryOver<Models.SystemSettings>().List<Models.SystemSettings>().FirstOrDefault();
            
            var json = new
            {
                ApplicationName = ApplicationSettings.GetApplicationName(),
                Version = version,
                ConstructorError = constructorError,
                DateFormat = systemSettings?.DateFormat ?? "dd-MM-yyyy" // IS this used or are we just using ISO Date Format
                //TODO
            };
            return json;
        }

        public async Task<object> InitializeSession()
        {
            var user = await QBicUtils.GetLoggedInUserAsync<User>(UserContext, HttpContextAccessor);
            if (user == null)
            {
                return new
                {
                    User = "",
                    Id = -1
                };
            }
            var json = new
            {
                User = user.UserName,
                Id = user.Id,
            };
            return json;
        }
    }
}