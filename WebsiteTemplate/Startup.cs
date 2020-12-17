using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.Unity.Configuration;
using QBic.Core.Data;
using QBic.Core.Utilities;
using Unity;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate
{
    public class Startup// : Microsoft.AspNetCore.Hosting.IStartup
    {
        //private static IServiceProvider Container { get; set; }
        public void Configure(IApplicationBuilder app)
        {
            
            var appSettings = app.ApplicationServices.GetService<ApplicationSettingsCore>();

            //var options = new UserAuthenticationOptions()
            //{
            //    AccessControlAllowOrigin = appSettings.AccessControlAllowOrigin,
            //    AccessTokenExpireTimeSpan = appSettings.AccessTokenExpireTimeSpan,
            //    RefreshTokenExpireTimeSpan = appSettings.RefreshTokenExpireTimeSpan,
            //    AllowInsecureHttp = false,
            //    TokenEndpointPath = new PathString(appSettings.TokenEndpointPath),
            //    UserContext = Container.GetService<UserContext>(),
            //    ClientId = appSettings.ClientId
            //};

            //app.UseBasicUserTokenAuthentication(options);
            
           // appSettings.PerformAdditionalStartupConfiguration(app, Container);

            app.ApplicationServices.GetService<SystemLogger>().Setup(appSettings.LogLevel);

        }

        public void ConfigureServices(IServiceCollection services)
        {
            var Container = new UnityContainer();
            Container.LoadConfiguration();

            var appSettings = Container.Resolve<ApplicationSettingsCore>();

#if (DEBUG)
            if (appSettings.DebugStartup)
            {
                if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            }
#endif

            services.AddSingleton(DataStore.GetInstance(false, null));// appSettings.UpdateDatabase));

            //services.AddScoped(typeof(ApplicationStartup), appSettings.GetApplicationStartupType);

            services.AddScoped<UserInjector, DefaultUserInjector>();

            //services.AddTransient(app.GetDataProtectionProvider());

            var container = services.BuildServiceProvider();
            var appStartup = container.GetService<ApplicationStartup>();
            appStartup.RegisterUnityContainers(container);

            appSettings.PerformAdditionalStartupConfiguration(services);

            //config.DependencyResolver = new UnityDependencyResolver(Container);

            //var cors = new EnableCorsAttribute("*", "*", "*");

            //config.EnableCors(cors);
        }
    }
}