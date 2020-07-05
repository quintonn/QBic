using BasicAuthentication.Authentication;
using BasicAuthentication.Security;
using BasicAuthentication.Startup;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Practices.Unity.Configuration;
using Owin;
using QBic.Core.Data;
using QBic.Core.Utilities;
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
            var appSettings = Container.Resolve<ApplicationSettingsCore>();

            var options = new UserAuthenticationOptions()
            {
                AccessControlAllowOrigin = appSettings.AccessControlAllowOrigin,
                AccessTokenExpireTimeSpan = appSettings.AccessTokenExpireTimeSpan,
                RefreshTokenExpireTimeSpan = appSettings.RefreshTokenExpireTimeSpan,
                AllowInsecureHttp = false,
                TokenEndpointPath = new PathString(appSettings.TokenEndpointPath),
                UserContext = Container.Resolve<UserContext>(),
                ClientId = appSettings.ClientId
            };

            app.UseBasicUserTokenAuthentication(options);
            
            appSettings.PerformAdditionalStartupConfiguration(app, Container);

            Container.Resolve<SystemLogger>().Setup(appSettings.LogLevel);

        }

        public void Register(HttpConfiguration config, IAppBuilder app)
        {
            Container = new UnityContainer();
            Container.LoadConfiguration();

            var appSettings = Container.Resolve<ApplicationSettingsCore>();

            #if (DEBUG)
            if (appSettings.DebugStartup)
            {
                if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            }
            #endif
            Container.RegisterInstance(DataStore.GetInstance(appSettings.UpdateDatabase));

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