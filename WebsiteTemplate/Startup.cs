using BasicAuthentication.Authentication;
using BasicAuthentication.Security;
using BasicAuthentication.Startup;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Owin;
using System;
using System.Web.Http;
using WebsiteTemplate.Data;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate
{
    public class Startup : IStartup
    {
        private static IUnityContainer Container { get; set; }
        public void Configuration(IAppBuilder app)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            var myApp = app;

            var appSettings = Container.Resolve<ApplicationSettingsCore>();

            var options = new UserAuthenticationOptions()
            {
                AccessControlAllowOrigin = "*",
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1), //Access token expires after 60min
                RefreshTokenExpireTimeSpan = TimeSpan.FromDays(7), //Refresh token expires after 7 days
                AllowInsecureHttp = false,
                TokenEndpointPath = new PathString("/api/v1/token"),
                UserContext = Container.Resolve<UserContext>()
            };

            myApp.UseBasicUserTokenAuthentication(options);
            
            appSettings.PerformAdditionalStartupConfiguration(app, Container);

            myApp.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
        }

        public void Register(HttpConfiguration config, IAppBuilder app)
        {
            //app.Use<GlobalExceptionMiddleware>("X", "Q", 10);

            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            //config.MapHttpAttributeRoutes();

            Container = new UnityContainer();
            Container.LoadConfiguration();

            var appSettings = Container.Resolve<ApplicationSettingsCore>();
            Container.RegisterInstance(DataStore.GetInstance(appSettings));

            Container.RegisterType(typeof(ApplicationStartup), appSettings.GetApplicationStartupType);

            Container.RegisterInstance(app.GetDataProtectionProvider());

            var appStartup = Container.Resolve<ApplicationStartup>();
            appStartup.RegisterUnityContainers(Container);

            config.DependencyResolver = new UnityDependencyResolver(Container);
        }
    }
}