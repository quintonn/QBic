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
        public void Configuration(IAppBuilder app)
        {
            var myApp = app;
            var options = new UserAuthenticationOptions()
            {
                AccessControlAllowOrigin = "*",
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30), //Access token expires after 30min
                RefreshTokenExpireTimeSpan = TimeSpan.FromMinutes(120), //Refresh token expires after 2 hours
                AllowInsecureHttp = false,
                TokenEndpointPath = new PathString("/api/v1/token"), //path is actually now /api/v1/token
                UserContext = new UserContext()
            };

            myApp.UseBasicUserTokenAuthentication(options);
            myApp.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
        }

        public void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var container = new UnityContainer();
            container.LoadConfiguration();
            container.RegisterInstance(DataStore.GetInstance());

            var appSettings = container.Resolve<IApplicationSettings>();
            appSettings.RegisterUnityContainers(container);

            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}