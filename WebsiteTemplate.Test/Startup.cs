using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QBic.Core.Utilities;
using System;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Test.MenuItems.Users;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test
{
    public class Startup
    {
        private IConfiguration Config;

        public Startup(IConfiguration config)
        {
            Config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var idOptions = new Action<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedAccount = true;

                options.User.AllowedUserNameCharacters += " ";
            });
            services.UseQBic<AppSettings, AppStartup>(Config, idOptions);
            //services.AddScoped<UserInjector, DefaultUserInjector>(); // can i override the default one?
            services.AddScoped<UserInjector, TestUserInjector>();
            //User can override UserInjector with their own injector class
        }


        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ILoggerFactory logFactory)
        {
            SystemLogger.Setup(logFactory);
            app.UseQBic(serviceProvider);
        }
    }
}