//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Core.Security;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BasicAuthentication.Core.Users
{
    public abstract class CoreUserContext<TUser> : ICoreUserContext where TUser : class, ICoreIdentityUser
    {
        public virtual void Dispose()
        {

        }

        public abstract void Initialize();

        public abstract Task CreateUserAsync(TUser user);
        public abstract Task DeleteUserAsync(TUser user);
        public abstract Task UpdateUserAsync(TUser user);
        public abstract Task<TUser> FindUserByIdAsync(string id);
        public abstract Task<TUser> FindUserByNameAsync(string name);
        public abstract Task<TUser> FindUserByLogin(UserLoginInfo login);
        public abstract IQueryable<TUser> GetUsers();
        public abstract Task<string> GetPasswordHashAsync(TUser user);
        public abstract Task<bool> HasPasswordAsync(TUser user);
        public abstract Task SetPasswordHashAsync(TUser user, string passwordHash);
        public abstract Task<string> GetSecurityStampAsync(TUser user);
        public abstract Task SetSecurityStampAsync(TUser user, string stamp);
        public abstract Task<TUser> FindUserByEmailAsync(string email);
        public abstract Task<string> GetEmailAsync(TUser user);
        public abstract Task<bool> GetEmailConfirmedAsync(TUser user);
        public abstract Task SetEmailAsync(TUser user, string email);
        public abstract Task SetEmailConfirmedAsync(TUser user, bool confirmed);
        public abstract Task AddClaimAsync(TUser user, Claim claim);
        public abstract Task<IList<Claim>> GetClaimsAsync(TUser user);
        public abstract Task RemoveClaimAsync(TUser user, Claim claim);
        public abstract Task AddRefreshToken(RefreshToken token);
        public abstract Task<RefreshToken> FindRefreshToken(string hashedTokenId);
        public abstract Task RemoveRefreshToken(string hashedTokenId);
        public abstract Task<bool> UserCanLogIn(string userId);
        public abstract Task AddToRoleAsync(ICoreIdentityUser user, string roleName);
        public abstract Task<IList<string>> GetRolesAsync(ICoreIdentityUser user);
        public abstract Task<bool> IsInRoleAsync(ICoreIdentityUser user, string roleName);
        public abstract Task RemoveFromRoleAsync(ICoreIdentityUser user, string roleName);

        void ICoreUserContext.Initialize()
        {
            this.Initialize();
        }

        Task ICoreUserContext.CreateUserAsync(ICoreIdentityUser user)
        {
            return CreateUserAsync(user as TUser);
        }

        Task ICoreUserContext.DeleteUserAsync(ICoreIdentityUser user)
        {
            return this.DeleteUserAsync(user as TUser);
        }

        Task ICoreUserContext.UpdateUserAsync(ICoreIdentityUser user)
        {
            return this.UpdateUserAsync(user as TUser);
        }

        Task<ICoreIdentityUser> ICoreUserContext.FindUserByIdAsync(string id)
        {
            var result = this.FindUserByIdAsync(id).Result;
            return Task.FromResult<ICoreIdentityUser>(result);
        }

        Task<ICoreIdentityUser> ICoreUserContext.FindUserByNameAsync(string name)
        {
            var result = FindUserByNameAsync(name).Result;
            return Task.FromResult<ICoreIdentityUser>(result);
        }

        Task<ICoreIdentityUser> ICoreUserContext.FindUserByLogin(UserLoginInfo login)
        {
            var result = FindUserByLogin(login).Result;
            return Task.FromResult<ICoreIdentityUser>(result);
        }

        IQueryable<ICoreIdentityUser> ICoreUserContext.GetUsers()
        {
            return GetUsers();
        }

        public Task<string> GetPasswordHashAsync(ICoreIdentityUser user)
        {
            return GetPasswordHashAsync(user as TUser);
        }

        public Task<bool> HasPasswordAsync(ICoreIdentityUser user)
        {
            return HasPasswordAsync(user as TUser);
        }

        Task ICoreUserContext.SetPasswordHashAsync(ICoreIdentityUser user, string passwordHash)
        {
            return SetPasswordHashAsync(user as TUser, passwordHash);
        }

        public Task<string> GetSecurityStampAsync(ICoreIdentityUser user)
        {
            return GetSecurityStampAsync(user as TUser);
        }

        Task ICoreUserContext.SetSecurityStampAsync(ICoreIdentityUser user, string stamp)
        {
            return SetSecurityStampAsync(user as TUser, stamp);
        }

        Task<ICoreIdentityUser> ICoreUserContext.FindUserByEmailAsync(string email)
        {
            var result = FindUserByEmailAsync(email).Result;
            return Task.FromResult<ICoreIdentityUser>(result);
        }

        public Task<string> GetEmailAsync(ICoreIdentityUser user)
        {
            return GetEmailAsync(user as TUser);
        }

        public Task<bool> GetEmailConfirmedAsync(ICoreIdentityUser user)
        {
            return GetEmailConfirmedAsync(user as TUser);
        }

        Task ICoreUserContext.SetEmailAsync(ICoreIdentityUser user, string email)
        {
            return SetEmailAsync(user as TUser, email);
        }

        Task ICoreUserContext.SetEmailConfirmedAsync(ICoreIdentityUser user, bool confirmed)
        {
            return SetEmailConfirmedAsync(user as TUser, confirmed);
        }

        Task ICoreUserContext.AddClaimAsync(ICoreIdentityUser user, Claim claim)
        {
            return AddClaimAsync(user as TUser, claim);
        }

        public Task<IList<Claim>> GetClaimsAsync(ICoreIdentityUser user)
        {
            return GetClaimsAsync(user as TUser);
        }

        Task ICoreUserContext.RemoveClaimAsync(ICoreIdentityUser user, Claim claim)
        {
            return RemoveClaimAsync(user as TUser, claim);
        }

        Task ICoreUserContext.AddRefreshToken(RefreshToken token)
        {
            return AddRefreshToken(token);
        }

        Task<RefreshToken> ICoreUserContext.FindRefreshToken(string hashedTokenId)
        {
            return FindRefreshToken(hashedTokenId);
        }

        Task ICoreUserContext.RemoveRefreshToken(string hashedTokenId)
        {
            return RemoveRefreshToken(hashedTokenId);
        }

        Task<bool> ICoreUserContext.UserCanLogIn(string userId)
        {
            return UserCanLogIn(userId);
        }

        Task ICoreUserContext.AddToRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return AddToRoleAsync(user, roleName);
        }

        Task<IList<string>> ICoreUserContext.GetRolesAsync(ICoreIdentityUser user)
        {
            return GetRolesAsync(user);
        }

        Task<bool> ICoreUserContext.IsInRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return IsInRoleAsync(user, roleName);
        }

        Task ICoreUserContext.RemoveFromRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return RemoveFromRoleAsync(user, roleName);
        }
    }
}