﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using QBic.Authentication;
using QBic.Core.Utilities;
using System.Threading.Tasks;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.Services
{
    public class ApplicationService
    {
        private static ApplicationSettingsCore ApplicationSettings { get; set; }
        private UserManager<User> UserContext { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; set; }
        public ApplicationService(ApplicationSettingsCore applicationSettings, UserManager<User> userContext, IHttpContextAccessor httpContextAccessor)
        {
            ApplicationSettings = applicationSettings;
            UserContext = userContext;
            HttpContextAccessor = httpContextAccessor;
        }

        
        public object InitializeApplication(string constructorError)
        {
            //var version = XXXUtils.GetApplicationCoreVersion().ToString();

            var version = ApplicationSettings.GetType().Assembly.GetName().Version.ToString();

            var json = new
            {
                ApplicationName = ApplicationSettings.GetApplicationName(),
                Version = version,
                ConstructorError = constructorError
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