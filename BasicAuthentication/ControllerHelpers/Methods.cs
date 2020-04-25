//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Core.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BasicAuthentication.ControllerHelpers
{
    public static class Methods
    {
        public static async Task<ICoreIdentityUser> GetLoggedInUserAsync(ICoreUserContext userContext)
        {
            IIdentity userIdentity = null;
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                userIdentity = HttpContext.Current.User.Identity;
            }
            if (userIdentity == null)
            {
                return null;
            }
            var user = await userContext.FindUserByNameAsync(userIdentity.Name);
            return user;
        }

        public static ICoreIdentityUser GetLoggedInUser(ICoreUserContext userContext)
        {
            IIdentity userIdentity = null;
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                userIdentity = HttpContext.Current.User.Identity;
            }
            if (userIdentity == null)
            {
                return null;
            }
            var result = userContext.FindUserByNameAsync(userIdentity.Name);
            result.Wait();
            if (result.Status != TaskStatus.RanToCompletion)
            {
                throw new Exception("Enable to complete async task in non-async mode:\n" + result.Status);
            }
            return result.Result;
        }
        public static async Task<ICoreIdentityUser> GetLoggedInUserAsync(HttpRequestContext requestContext, ICoreUserContext userContext)
        {
            var user = requestContext.Principal.Identity;
            var result = await userContext.FindUserByNameAsync(user.Name);
            return result;
        }

        public static ICoreIdentityUser GetLoggedInUser(HttpRequestContext requestContext, ICoreUserContext userContext)
        {

            var user = requestContext.Principal.Identity;
            //var result = CoreAuthenticationEngine.UserManager.FindByNameAsync(user.Name);
            var result = userContext.FindUserByNameAsync(user.Name);
            result.Wait();
            if (result.Status != TaskStatus.RanToCompletion)
            {
                throw new Exception("Enable to complete async task in non-async mode:\n" + result.Status);
            }
            return result.Result;
        }

        public static async Task<ICoreIdentityUser> GetLoggedInUserAsync(this ApiController controller, ICoreUserContext userContext)
        {
            return await GetLoggedInUserAsync(controller.RequestContext, userContext);
        }

        public static ICoreIdentityUser GetLoggedInUser(this ApiController controller, ICoreUserContext userContext)
        {
            return GetLoggedInUser(controller.RequestContext, userContext);
        }

        public static IDictionary<string, string> ParseFormData(string data)
        {
            var items = data.Split("&".ToCharArray());
            var result = new Dictionary<string, string>();
            foreach (var item in items)
            {
                var parts = item.Split("=".ToCharArray());
                var key = HttpUtility.UrlDecode(parts.First());
                var value = HttpUtility.UrlDecode(parts.Last());

                result.Add(key, value);
            }
            return result;
        }

        public static IDictionary<string, string> ParseFormData(this ApiController controller, string data)
        {
            var items = data.Split("&".ToCharArray());
            var result = new Dictionary<string, string>();
            foreach (var item in items)
            {
                var parts = item.Split("=".ToCharArray());
                var key = HttpUtility.UrlDecode(parts.First());
                var value = HttpUtility.UrlDecode(parts.Last());

                result.Add(key, value);
            }
            return result;
        }
    }
}
