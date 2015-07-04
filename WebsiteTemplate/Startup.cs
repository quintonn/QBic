using BasicAuthentication.Security;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicAuthentication.Authentication;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
using WebsiteTemplate.Data;

[assembly: OwinStartup(typeof(WebsiteTemplate.Startup))]
namespace WebsiteTemplate
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var myApp = app;
            //app.Map("/jbq", myApp =>
              //  {
                    var options = new UserAuthenticationOptions()
                    {
                        AccessControlAllowOrigin = "*",
                        AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(2), //Refresh the token every 30 minutes
                        AllowInsecureHttp = false,
                        TokenEndpointPath = new PathString("/api/v1/token"), //path is actually now /jbq/web/api/token
                        UserContext = new UserContext()
                    };
                    myApp.UseBasicUserTokenAuthentication(options);

                    var configuration = new HttpConfiguration();
                    configuration.MapHttpAttributeRoutes();

                    var jsonFormatter = configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();
                    //jsonFormatter.SerializerSettings.ContractResolver =  new CamelCasePropertyNamesContractResolver();

                    myApp.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
                    myApp.UseWebApi(configuration);
               // });
        }
    }
}