//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Authentication;
using BasicAuthentication.Security;
using BasicAuthentication.Startup;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace BasicAuthentication.WebTest
{
    public class Startup : IStartup
    {
        public void Configuration(IAppBuilder app)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();

            var myApp = app;
            var options = new UserAuthenticationOptions()
            {
                AccessControlAllowOrigin = "*",
                AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(8), //Access tokens should be short lived
                RefreshTokenExpireTimeSpan = TimeSpan.FromMinutes(10), // Refresh token should be long lived but stored securely
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/v1/token"), //path is actually now /api/v1/token
                UserContext = new UserContext(app.GetDataProtectionProvider()),
                ClientId = "BasicAuthTest"
            };

            myApp.UseBasicUserTokenAuthentication(options);

            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();

            var jsonFormatter = configuration.Formatters.OfType<JsonMediaTypeFormatter>().First();

            myApp.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            myApp.UseWebApi(configuration);
        }

        public void Register(HttpConfiguration config, IAppBuilder app)
        {

        }
    }
}