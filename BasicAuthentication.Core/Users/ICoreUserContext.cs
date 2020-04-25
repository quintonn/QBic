//  Copyright 2017 Quintonn Rothmann
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using BasicAuthentication.Core.Security;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicAuthentication.Core.Users
{
    public interface ICoreUserContext
    {
        void Dispose();

        void Initialize();

        Task AddRefreshToken(RefreshToken token);

        Task<RefreshToken> FindRefreshToken(string hashedTokenId);

        Task RemoveRefreshToken(string hashedTokenId);

        Task<bool> UserCanLogIn(string userId);

        Task CreateUserAsync(ICoreIdentityUser user);

        Task DeleteUserAsync(ICoreIdentityUser user);

        Task UpdateUserAsync(ICoreIdentityUser user);

        Task<ICoreIdentityUser> FindUserByIdAsync(string id);

        Task<ICoreIdentityUser> FindUserByNameAsync(string name);

        Task<ICoreIdentityUser> FindUserByLogin(UserLoginInfo login);

        IQueryable<ICoreIdentityUser> GetUsers();

        #region Password Methods

        Task<string> GetPasswordHashAsync(ICoreIdentityUser user);

        Task<bool> HasPasswordAsync(ICoreIdentityUser user);

        Task SetPasswordHashAsync(ICoreIdentityUser user, string passwordHash);

        #endregion

        #region Security Stamp Methods

        Task<string> GetSecurityStampAsync(ICoreIdentityUser user);

        Task SetSecurityStampAsync(ICoreIdentityUser user, string stamp);

        #endregion

        #region Email Methods

        Task<ICoreIdentityUser> FindUserByEmailAsync(string email);

        Task<string> GetEmailAsync(ICoreIdentityUser user);

        Task<bool> GetEmailConfirmedAsync(ICoreIdentityUser user);

        Task SetEmailAsync(ICoreIdentityUser user, string email);

        Task SetEmailConfirmedAsync(ICoreIdentityUser user, bool confirmed);

        #endregion

        #region Claims Methods

        Task AddClaimAsync(ICoreIdentityUser user, System.Security.Claims.Claim claim);

        Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(ICoreIdentityUser user);

        Task RemoveClaimAsync(ICoreIdentityUser user, System.Security.Claims.Claim claim);

        #endregion

        #region User Role Methods

        Task AddToRoleAsync(ICoreIdentityUser user, string roleName);

        Task<IList<string>> GetRolesAsync(ICoreIdentityUser user);

        Task<bool> IsInRoleAsync(ICoreIdentityUser user, string roleName);

        Task RemoveFromRoleAsync(ICoreIdentityUser user, string roleName);

        #endregion
    }
}
