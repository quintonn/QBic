using BasicAuthentication.Authentication;
using BasicAuthentication.Security;
using BasicAuthentication.Startup;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Practices.Unity.Configuration;
using Owin;
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using Unity;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Data;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate
{
    public class Startup : IStartup
    {
        private static IUnityContainer Container { get; set; }
        public void Configuration(IAppBuilder app)
        {
            //app.Use(async (context, next) =>
            //{
            //    var request = context.Request;
                
            //    await next();
            //});


            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            //var myApp = app;

            var appSettings = Container.Resolve<ApplicationSettingsCore>();

            var options = new UserAuthenticationOptions()
            {
                AccessControlAllowOrigin = "*",
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1), //Access token expires after 60min
                RefreshTokenExpireTimeSpan = TimeSpan.FromDays(7), //Refresh token expires after 7 days
                AllowInsecureHttp = false,
                TokenEndpointPath = new PathString("/api/v1/token"),
                UserContext = Container.Resolve<UserContext>(),
                ClientId = appSettings.ClientId
            };

            app.UseBasicUserTokenAuthentication(options);
            
            appSettings.PerformAdditionalStartupConfiguration(app, Container);

            //app.UseCors(CorsOptions.AllowAll);
            
        }

        public void Register(HttpConfiguration config, IAppBuilder app)
        {
            //app.Use<GlobalExceptionMiddleware>("X", "Q", 10);

            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            //config.MapHttpAttributeRoutes();

            Container = new UnityContainer();
            Container.LoadConfiguration();

            var appSettings = Container.Resolve<ApplicationSettingsCore>();

            #if (DEBUG)
            if (appSettings.DebugStartup)
            {
                if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            }
            #endif
            Container.RegisterInstance(DataStore.GetInstance(appSettings));

            Container.RegisterType(typeof(ApplicationStartup), appSettings.GetApplicationStartupType);

            Container.RegisterType<UserInjector, DefaultUserInjector>();

            Container.RegisterInstance(app.GetDataProtectionProvider());

            var appStartup = Container.Resolve<ApplicationStartup>();
            appStartup.RegisterUnityContainers(Container);

            config.DependencyResolver = new UnityDependencyResolver(Container);

            var cors = new EnableCorsAttribute("*", "*", "*");

            config.EnableCors(cors);
        }
    }
}