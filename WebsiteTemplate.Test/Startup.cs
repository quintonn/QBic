using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            var appSettings = new AppSettings();
            
            services.UseQBic<AppSettings, AppStartup>(Config);

#if (DEBUG)
            if (appSettings.DebugStartup)
            {
                if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            }
#endif

            //services.AddScoped<UserInjector, DefaultUserInjector>(); // can i override the default one?
            //User can override UserInjector with their own injector class
        }


        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseQBic(serviceProvider);
        }
    }
}