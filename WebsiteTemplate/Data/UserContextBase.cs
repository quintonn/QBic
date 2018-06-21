using BasicAuthentication.Core.Security;
using BasicAuthentication.Core.Users;
using QBic.Core.Data;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Data
{
    public class UserContextBase<T> : CoreUserContext<T> where T : UserBase
    {
        protected DataStore DataStore { get; set; } // This is only here because the OpenSession is a mess and not working
        public UserContextBase(DataStore dataStore)
        {
            DataStore = dataStore;
        }

        public override void Initialize()
        {

        }

        public override Task<bool> UserCanLogIn(string userId)
        {
            using (var session = DataStore.OpenSession())
            {
                var user = session.Get<T>(userId);
                if (user == null)
                {
                    return Task.FromResult(false);
                }
                if (user.UserStatus == UserStatus.Locked)
                {
                    return Task.FromResult(false);
                }
                
                session.Flush();
            }
            return Task.FromResult(true);
        }

        public override Task AddClaimAsync(T user, System.Security.Claims.Claim claim)
        {
            return Task.FromResult(0);
        }

        public override Task AddRefreshToken(RefreshToken token)
        {
            using (var session = DataStore.OpenSession())
            {
                session.SaveOrUpdate(token);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override Task CreateUserAsync(T user)
        {
            using (var session = DataStore.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override Task DeleteUserAsync(T user)
        {
            using (var session = DataStore.OpenSession())
            {
                session.Delete(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override Task<RefreshToken> FindRefreshToken(string hashedTokenId)
        {
            RefreshToken token;
            using (var session = DataStore.OpenSession())
            {
                token = session.QueryOver<RefreshToken>().Where(r => r.Id == hashedTokenId).SingleOrDefault();
            }
            return Task.FromResult<RefreshToken>(token);
        }

        public override Task<T> FindUserByEmailAsync(string email)
        {
            T result;
            using (var session = DataStore.OpenSession())
            {
                //result = session.Query<T>().Where(u => u.Email == email).FirstOrDefault();
                result = session.CreateCriteria<T>()
                                .Add(Restrictions.Eq("Email", email).IgnoreCase())
                                .UniqueResult<T>();
                session.Flush();
            }
            return Task.FromResult(result);
        }

        public override Task<T> FindUserByIdAsync(string id)
        {
            T result;
            using (var session = DataStore.OpenSession())
            {
                result = session.Get<T>(id);
                session.Flush();
            }
            return Task.FromResult(result);
        }

        public override Task<T> FindUserByLogin(Microsoft.AspNet.Identity.UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public override Task<T> FindUserByNameAsync(string name)
        {
            T result;
            using (var session = DataStore.OpenSession())
            {
                result = session.CreateCriteria<T>()
                                .Add(Restrictions.Eq("UserName", name).IgnoreCase())
                                .UniqueResult<T>();
            }
            return Task.FromResult(result);
        }

        public override Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(T user)
        {
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public override Task<string> GetEmailAsync(T user)
        {
            return Task.FromResult(user.Email);
        }

        public override Task<bool> GetEmailConfirmedAsync(T user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public override Task<string> GetPasswordHashAsync(T user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public override Task<string> GetSecurityStampAsync(T user)
        {
            var result = String.Empty;
            if (SecurityStamps.ContainsKey(user.Id))
            {
                result = SecurityStamps[user.Id];
            }
            return Task.FromResult(result);
        }

        public override IQueryable<T> GetUsers()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> HasPasswordAsync(T user)
        {
            return Task.FromResult(true);
        }

        public override Task RemoveClaimAsync(T user, System.Security.Claims.Claim claim)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveRefreshToken(string hashedTokenId)
        {
            using (var session = DataStore.OpenSession())
            {
                var token = session.QueryOver<RefreshToken>().Where(r => r.Id == hashedTokenId).SingleOrDefault();
                session.Delete(token);
                session.Flush();
            }
            return Task.FromResult<RefreshToken>(null);
        }

        public override Task SetEmailAsync(T user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public override Task SetEmailConfirmedAsync(T user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public override Task SetPasswordHashAsync(T user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public override Task SetSecurityStampAsync(T user, string stamp)
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

        private Dictionary<string, string> SecurityStamps = new Dictionary<string, string>();

        public override Task UpdateUserAsync(T user)
        {
            using (var session = DataStore.OpenSession())
            {
                var dbUser = session.Get<T>(user.Id);
                var properties = dbUser.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.Name.Equals("Id", StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    
                    var value = property.GetValue(user);
                    try
                    {
                        property.SetValue(dbUser, value);
                    }
                    catch (Exception ex)
                    {
                        var cont = false;
                        do
                        {
                            if (ex.Message == "Property set method not found.")
                            {
                                cont = true;
                                break;
                                //This is ok, it means there is not SET method on the property
                            }
                            ex = ex.InnerException;
                        }
                        while (ex.InnerException != null);

                        if (cont)
                        {
                            continue;
                        }

                        throw;
                    }
                }
                session.SaveOrUpdate(dbUser);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override Task AddToRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return Task.FromResult(0);
        }

        public override Task<IList<string>> GetRolesAsync(ICoreIdentityUser user)
        {
            var results = new List<string>();
            return Task.FromResult<IList<string>>(results);
        }

        public override Task<bool> IsInRoleAsync(ICoreIdentityUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public override Task RemoveFromRoleAsync(ICoreIdentityUser user, string roleName)
        {
            return Task.FromResult(0);
        }
    }
}