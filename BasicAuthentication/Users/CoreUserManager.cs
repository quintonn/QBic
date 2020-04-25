//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Core.Security;
using BasicAuthentication.Core.Users;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Threading.Tasks;

namespace BasicAuthentication.Users
{
    /// <summary>
    /// A simple implementation of the Microsoft.AspNet.Identity.UserManager&lt;TUser&gt; class.
    /// </summary>
    public class CoreUserManager : UserManager<ICoreIdentityUser>
    {
        public ICoreUserContext UserContext { get; set; }

        /// <summary>
        /// This creates an instance of CoreUserManager
        /// </summary>
        /// <param name="userContext">An implementation of the BasicAuthentication.Users.UserContext abstract class. </param>
        /// <param name="provider">An instance of a Microsoft.AspNet.IdentityIUserTokenProvider &lt; TUser, string &gt; class which is used to provide password reset tokens.</param>
        /// <example>
        /// An Microsoft.AspNet.IdentityIUserTokenProvider &lt; TUser string &gt; can be created using a Microsoft.Owin.Security.DataProtection.IDataProtectionProvider as follows:
        /// <code>
        /// Microsoft.Owin.Security.DataProtection.IDataProtectionProvider provider; This needs to be created, here for reference.
        /// var tokenProvider = new Microsoft.AspNet.Identity.Owin.DataProtectorTokenProvider&lt;BasicAuthentication.Users.IdentityUser&gt;(provider.Create("EmailConfirmation"));
        /// </code>
        /// </example>
        public CoreUserManager(ICoreUserContext userContext, IUserTokenProvider<ICoreIdentityUser, string> provider)
            : base(new UserStore<ICoreIdentityUser>(userContext))
        {
            this.UserContext = userContext;
            this.UserTokenProvider = provider;

            this.UserValidator = new UserValidator<ICoreIdentityUser>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
            };
        }

        /// <summary>
        /// This creates an instance of CoreUserManager
        /// </summary>
        /// <param name="userContext">An implementation of the BasicAuthentication.Users.UserContext abstract class. </param>
        /// <param name="provider">A Microsoft.Owin.Security.DataProtection.IDataProtectionProvider instance used to create a Microsoft.AspNet.IdentityIUserTokenProvider &lt; TUser, string &gt;.</param>
        /// <example>
        /// A Microsoft.Owin.Security.DataProtection.IDataProtectionProvider can be created inside an OWIN startup class as follows:
        /// <code>app.GetDataProtectionProvider();</code>
        /// </example>
        public CoreUserManager(ICoreUserContext userContext, IDataProtectionProvider provider)
            : base(new UserStore<ICoreIdentityUser>(userContext))
        {
            this.UserContext = userContext;

            //TODO:
            // token provider and user manager needs to be static i think:
            // https://stackoverflow.com/questions/25405307/asp-net-identity-2-giving-invalid-token-error

            var tokenProvider = new DataProtectorTokenProvider<ICoreIdentityUser>(provider.Create("EmailConfirmation", "ResetPassword"))
            {
                TokenLifespan = TimeSpan.FromDays(7)
            };
            this.UserTokenProvider = tokenProvider;

            this.UserValidator = new UserValidator<ICoreIdentityUser>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
            };
        }

        public Task AddRefreshToken(RefreshToken token)
        {
            return UserContext.AddRefreshToken(token);
        }

        public Task<RefreshToken> FindRefreshToken(string hashedTokenId)
        {
            return UserContext.FindRefreshToken(hashedTokenId);
        }

        public override Task<string> GeneratePasswordResetTokenAsync(string userId)
        {
            return base.GeneratePasswordResetTokenAsync(userId);
        }

        public Task RemoveRefreshToken(string hashedTokenId)
        {
            return UserContext.RemoveRefreshToken(hashedTokenId);
        }

        public Task<bool> UserCanLogIn(string userId)
        {
            return UserContext.UserCanLogIn(userId);
        }
    }
}
