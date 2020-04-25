//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Core.Security;
using BasicAuthentication.Users;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BasicAuthentication.Security
{
    public class SimpleRefreshTokenProvider : IAuthenticationTokenProvider
    {
        private CoreUserManager UserManager { get; set; }

        private TimeSpan TokenLifeSpan { get; set; }

        private string AccessControlAllowOrigin { get; set; }

        public SimpleRefreshTokenProvider(CoreUserManager userManager, TimeSpan tokenLifeSpan, string accessControlAllowOrigin)
        {
            UserManager = userManager;
            TokenLifeSpan = tokenLifeSpan;
            AccessControlAllowOrigin = accessControlAllowOrigin;
        }

        private string GetHash(string input)
        {
            var hashAlgorithm = new SHA256CryptoServiceProvider();

            var byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            var byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary["as:client_id"];

            if (string.IsNullOrEmpty(clientid))
            {
                return;
            }

            var refreshTokenId = Guid.NewGuid().ToString("n");

            var token = new RefreshToken()
            {
                Id = GetHash(refreshTokenId),
                ClientId = clientid,
                Subject = context.Ticket.Identity.Name,
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(TokenLifeSpan)
            };

            context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
            context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

            token.ProtectedTicket = context.SerializeTicket();

            await UserManager.AddRefreshToken(token);
            context.SetToken(refreshTokenId);
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            if (String.IsNullOrWhiteSpace(AccessControlAllowOrigin))
            {
                AccessControlAllowOrigin = "*";
            }

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { AccessControlAllowOrigin });
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });

            var hashedTokenId = GetHash(context.Token);

            var refreshToken = await UserManager.FindRefreshToken(hashedTokenId);

            if (refreshToken != null)
            {
                context.DeserializeTicket(refreshToken.ProtectedTicket);

                await UserManager.RemoveRefreshToken(hashedTokenId);
            }
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            var result = CreateAsync(context);
            result.Wait();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            var result = ReceiveAsync(context);
            result.Wait();
        }
    }
}
