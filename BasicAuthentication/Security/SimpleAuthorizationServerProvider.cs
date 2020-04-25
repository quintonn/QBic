//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Users;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BasicAuthentication.Security
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private CoreUserManager UserManager { get; set; }

        private string AccessControlAllowOrigin { get; set; }
        private string ClientId { get; set; }

        public SimpleAuthorizationServerProvider(CoreUserManager userManager, string accessControlAllowOrigin, string clientId)
        {
            UserManager = userManager;
            AccessControlAllowOrigin = accessControlAllowOrigin;
            ClientId = clientId;

        }

        public SimpleAuthorizationServerProvider()
        {
            this.OnValidateClientRedirectUri = ValidateClientRedirectUri;
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            return Task.FromResult(0);
        }

        public override Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            return base.AuthorizeEndpoint(context);
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            var clientId = default(string);
            var clientSecret = default(string);

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                context.Validated();

                context.SetError("invalid_clientId", "ClientId should be sent.");

                return Task.FromResult<object>(null);
            }

            if (context.ClientId != ClientId)
            {
                context.Validated();

                context.SetError("invalid_clientId", "Incorrect client id provided.");

                return Task.FromResult<object>(null);
            }

            context.Validated();

            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (String.IsNullOrWhiteSpace(AccessControlAllowOrigin))
            {
                AccessControlAllowOrigin = "*";
            }

            // If i want to obtain other values from the request, i can do that here.
            // For example, to get an email/username and a pin, instead of password.
            // Or i can just use those fields as they are.....i like this idea.
            //context.Request.ReadFormAsync

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { AccessControlAllowOrigin });
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });

            var dbUser = await UserManager.FindAsync(context.UserName, context.Password);
            
            if (dbUser == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                
                if (UserManager.SupportsUserLockout) //ABCxyz
                {
                    var user = await UserManager.FindByNameAsync(context.UserName);
                    if (user != null)
                    {
                        await UserManager.AccessFailedAsync(user.Id); // I am not supporting this feature anyway at the moment. Maybe i should???
                    }
                }
                return;
            }

            if (UserManager.SupportsUserEmail)
            {
                var emailConfirmed = await UserManager.IsEmailConfirmedAsync(dbUser.Id);
                
                if (emailConfirmed == false)
                {
                    context.SetError("invalid_grant", "Email has not been confirmed");
                    return;
                }
            }

            //UserContext.UserCanLogIn abcxyz
            var userCanLogIn = await UserManager.UserCanLogIn(dbUser.Id);
            if (!userCanLogIn)
            {
                context.SetError("invalid_grant", "User is not allowed to log in.");
                return;
            }
            
            var identity = await UserManager.CreateIdentityAsync(dbUser, context.Options.AuthenticationType);

            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { 
                        "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId
                    },
                    { 
                        "userName", context.UserName
                    }
                });

            var ticket = new AuthenticationTicket(identity, props);

            context.Validated(ticket);
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");

                return Task.FromResult<object>(null);
            }

            if (currentClient != ClientId)
            {
                context.SetError("invalid_clientId", "Incorrect client id found in token.");

                return Task.FromResult<object>(null);
            }

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            
            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }

    public class CustomContext: OAuthGrantResourceOwnerCredentialsContext
    {
        public CustomContext(IOwinContext context, OAuthAuthorizationServerOptions options, string clientId, string userName, string password, IList<string> scope)
            :base(context, options, clientId, userName, password, scope)
        {

        }

        public string Code { get; set; }
    }
}
