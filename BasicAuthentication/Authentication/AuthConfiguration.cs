//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Security;
using BasicAuthentication.Users;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;

namespace BasicAuthentication.Authentication
{
    public static class AuthConfiguration
    {
        public static void UseBasicUserTokenAuthentication(this IAppBuilder app, UserAuthenticationOptions userAuthenticationOptions)
        {
            /* Remove ability to have auth token in URL */
            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.QueryString.HasValue)
            //    {
            //        if (String.IsNullOrWhiteSpace(context.Request.Headers.Get("Authorization")))
            //        {
            //            var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
            //            string token = queryString.Get("token");

            //            if (!String.IsNullOrWhiteSpace(token))
            //            {
            //                context.Request.Headers.Add("Authorization", new[] { string.Format("Bearer {0}", token) });
            //            }
            //        }
            //    }

            //    await next.Invoke();
            //});

            var userManager = new CoreUserManager(userAuthenticationOptions.UserContext, app.GetDataProtectionProvider());
            var accessTokenLifeSpan = userAuthenticationOptions.AccessTokenExpireTimeSpan;
            var refreshTokenLifeSpan = userAuthenticationOptions.RefreshTokenExpireTimeSpan;
            var accessControlAllowOrigin = userAuthenticationOptions.AccessControlAllowOrigin;
            var clientId = userAuthenticationOptions.ClientId;

            userManager.PasswordValidator = userAuthenticationOptions.PasswordValidator;
            
            var OAuthServerOptions = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = userAuthenticationOptions.AllowInsecureHttp,
                TokenEndpointPath = userAuthenticationOptions.TokenEndpointPath,
                AccessTokenExpireTimeSpan = accessTokenLifeSpan,
                ApplicationCanDisplayErrors = true,
                Provider = new SimpleAuthorizationServerProvider(userManager, accessControlAllowOrigin, clientId),
                RefreshTokenProvider = new SimpleRefreshTokenProvider(userManager, refreshTokenLifeSpan, accessControlAllowOrigin),
            };

            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
            {
                AccessTokenProvider = OAuthServerOptions.RefreshTokenProvider,
                AccessTokenFormat = OAuthServerOptions.AccessTokenFormat
            });
            
            userManager.UserContext.Initialize();
        }
    }
}
