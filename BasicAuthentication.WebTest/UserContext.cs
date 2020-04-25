//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Core.Security;
using BasicAuthentication.Core.Users;
using BasicAuthentication.Users;
using BasicAuthentication.WebTest.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BasicAuthentication.WebTest
{
    public class UserContext : CoreUserContext<TestUser>
    {
        private List<RefreshToken> Tokens { get; set; }
        private List<TestUser> Users { get; set; }
        private Dictionary<string, string> SecurityStamps = new Dictionary<string, string>();

        public UserContext(IDataProtectionProvider protectionProvider)
        {
            Tokens = new List<RefreshToken>();
            Users = new List<TestUser>();

            Setup(protectionProvider);
        }

        private void Setup(IDataProtectionProvider protectionProvider)
        {
            new CoreUserManager(this, protectionProvider).Create(new TestUser()
            {
                UserName = "Steve",
                Email = "Steve@mail.com",
                EmailConfirmed = true
            }, "password");
        }

        public override Task AddClaimAsync(TestUser user, Claim claim)
        {
            return Task.FromResult(0);
        }

        public override Task AddRefreshToken(RefreshToken token)
        {
            Tokens.Add(token);
            return Task.FromResult(0);

        }

        public override Task AddToRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return Task.FromResult(0);
        }

        public override Task CreateUserAsync(TestUser user)
        {
            user.Id = Guid.NewGuid().ToString();
            Users.Add(user);
            return Task.FromResult(0);
        }

        public override Task DeleteUserAsync(TestUser user)
        {
            var delUser = Users.Where(u => u.Id == user.Id).FirstOrDefault();
            Users.Remove(delUser);
            return Task.FromResult(0);
        }

        public override Task<RefreshToken> FindRefreshToken(string hashedTokenId)
        {
            var token = Tokens.Where(t => t.Id == hashedTokenId).SingleOrDefault();
            return Task.FromResult<RefreshToken>(token);
        }

        public override Task<TestUser> FindUserByEmailAsync(string email)
        {
            var result = Users.Where(u => u.Email == email).FirstOrDefault();
            return Task.FromResult(result);
        }

        public override Task<TestUser> FindUserByIdAsync(string id)
        {
            var result = Users.Where(u => u.Id == id).FirstOrDefault();
            return Task.FromResult(result);
        }

        public override Task<TestUser> FindUserByLogin(UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public override Task<TestUser> FindUserByNameAsync(string name)
        {
            var result = Users.Where(u => u.UserName == name).FirstOrDefault();
            return Task.FromResult(result);
        }

        public override Task<IList<Claim>> GetClaimsAsync(TestUser user)
        {
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public override Task<string> GetEmailAsync(TestUser user)
        {
            return Task.FromResult(user.Email);
        }

        public override Task<bool> GetEmailConfirmedAsync(TestUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public override Task<string> GetPasswordHashAsync(TestUser user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public override Task<IList<string>> GetRolesAsync(ICoreIdentityUser user)
        {
            var results = new List<string>();
            return Task.FromResult<IList<string>>(results);
        }

        public override Task<string> GetSecurityStampAsync(TestUser user)
        {
            var result = String.Empty;
            if (SecurityStamps.ContainsKey(user.Id))
            {
                result = SecurityStamps[user.Id];
            }
            return Task.FromResult(result);
        }

        public override IQueryable<TestUser> GetUsers()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> HasPasswordAsync(TestUser user)
        {
            return Task.FromResult(true);
        }

        public override void Initialize()
        {
            
        }

        public override Task<bool> IsInRoleAsync(ICoreIdentityUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveClaimAsync(TestUser user, Claim claim)
        {
            return Task.FromResult(0);
        }

        public override Task RemoveFromRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return Task.FromResult(0);
        }

        public override Task RemoveRefreshToken(string hashedTokenId)
        {
            var token = Tokens.Where(t => t.Id == hashedTokenId).SingleOrDefault();
            if (token != null)
            {
                Tokens.Remove(token);
            }
            return Task.FromResult<RefreshToken>(null);
        }

        public override System.Threading.Tasks.Task SetEmailAsync(TestUser user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task SetEmailConfirmedAsync(TestUser user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task SetPasswordHashAsync(TestUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public override Task SetSecurityStampAsync(TestUser user, string stamp)
        {
            if (user != null && !String.IsNullOrWhiteSpace(user.Id))
            {
                if (!SecurityStamps.ContainsKey(user.Id))
                {
                    SecurityStamps.Add(user.Id, String.Empty);
                }
                SecurityStamps[user.Id] = stamp;
            }
            return Task.FromResult(0);
        }

        public override Task UpdateUserAsync(TestUser user)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> UserCanLogIn(string userId)
        {
            return Task.FromResult(true);
        }
    }
}