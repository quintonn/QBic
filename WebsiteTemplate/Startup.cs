using BasicAuthentication.Authentication;
using BasicAuthentication.Security;
using BasicAuthentication.Startup;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Owin;
using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebsiteTemplate.Data;
using Microsoft.Practices.Unity.Configuration;
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

            var options = new UserAuthenticationOptions()
            {
                AccessControlAllowOrigin = "*",
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30), //Access token expires after 30min
                RefreshTokenExpireTimeSpan = TimeSpan.FromMinutes(120), //Refresh token expires after 2 hours
                AllowInsecureHttp = false,
                TokenEndpointPath = new PathString("/api/v1/token"), //path is actually now /api/v1/token
                UserContext = Container.Resolve<UserContext>()
            };

            myApp.UseBasicUserTokenAuthentication(options);
            myApp.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
        }

        public void Register(HttpConfiguration config)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            config.MapHttpAttributeRoutes();

            Container = new UnityContainer();
            Container.LoadConfiguration();

            Container.RegisterInstance(DataStore.GetInstance());

            var appSettings = Container.Resolve<IApplicationSettings>();
            appSettings.RegisterUnityContainers(Container);

            config.DependencyResolver = new UnityDependencyResolver(Container);
        }
    }
}