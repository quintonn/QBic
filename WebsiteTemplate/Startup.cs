using BasicAuthentication.Authentication;
using BasicAuthentication.Security;
using BasicAuthentication.Startup;
using Microsoft.Owin;
using Owin;
using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using WebsiteTemplate.Data;

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

            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();

            var jsonFormatter = configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();

            myApp.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            myApp.UseWebApi(configuration);
        }
    }
}