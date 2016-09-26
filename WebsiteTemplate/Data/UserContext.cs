using BasicAuthentication.Security;
using BasicAuthentication.Users;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Data
{
    public class UserContext : CoreUserContext<User>
    {
        private DataService DataService { get; set; }
        private DataStore DataStore { get; set; } // This is only here because the OpenSession is a mess and not working
        public UserContext(DataService dataService, DataStore dataStore)
        {
            DataService = dataService;
            DataStore = dataStore;
        }

        public override void Initialize()
        {

        }

        public override Task<bool> UserCanLogIn(string userId)
        {
            using (var session = DataStore.OpenSession())
            {
                var user = session.Get<User>(userId);
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

        public override System.Threading.Tasks.Task AddClaimAsync(User user, System.Security.Claims.Claim claim)
        {
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task AddRefreshToken(BasicAuthentication.Security.RefreshToken token)
        {
            using (var session = DataStore.OpenSession())
            {
                session.SaveOrUpdate(token);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task CreateUserAsync(User user)
        {
            using (var session = DataService.OpenSession())
            {
                DataService.SaveOrUpdate(session, user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task DeleteUserAsync(User user)
        {
            using (var session = DataService.OpenSession())
            {
                DataService.TryDelete(session, user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task<BasicAuthentication.Security.RefreshToken> FindRefreshToken(string hashedTokenId)
        {
            RefreshToken token;
            using (var session = DataStore.OpenSession())
            {
                token = session.QueryOver<RefreshToken>().Where(r => r.Id == hashedTokenId).SingleOrDefault();
            }
               return Task.FromResult<RefreshToken>(token);
        }

        public override System.Threading.Tasks.Task<User> FindUserByEmailAsync(string email)
        {
            User result;
            using (var session = DataStore.OpenSession())
            {
                //result = session.Query<User>().Where(u => u.Email == email).FirstOrDefault();
                result = session.CreateCriteria<User>()
                                .Add(Restrictions.Eq("Email", email).IgnoreCase())
                                .UniqueResult<User>();
                session.Flush();
            }
            return Task.FromResult(result);
        }

        public override System.Threading.Tasks.Task<User> FindUserByIdAsync(string id)
        {
            User result;
            using (var session = DataStore.OpenSession())
            {
                result = session.Get<User>(id);
                session.Flush();
            }
            return Task.FromResult(result);
        }

        public override System.Threading.Tasks.Task<User> FindUserByLogin(Microsoft.AspNet.Identity.UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public override System.Threading.Tasks.Task<User> FindUserByNameAsync(string name)
        {
            User result;
            using (var session = DataStore.OpenSession())
            {
                result = session.CreateCriteria<User>()
                                .Add(Restrictions.Eq("UserName", name).IgnoreCase())
                                .UniqueResult<User>();
            }
            return Task.FromResult(result);
        }

        public override System.Threading.Tasks.Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(User user)
        {
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public override System.Threading.Tasks.Task<string> GetEmailAsync(User user)
        {
            return Task.FromResult(user.Email);
        }

        public override System.Threading.Tasks.Task<bool> GetEmailConfirmedAsync(User user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public override System.Threading.Tasks.Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public override System.Threading.Tasks.Task<string> GetSecurityStampAsync(User user)
        {
            var result = String.Empty;
            if (SecurityStamps.ContainsKey(user.Id))
            {
                result = SecurityStamps[user.Id];
            }
            return Task.FromResult(result);
        }

        public override IQueryable<User> GetUsers()
        {
            throw new NotImplementedException();
        }

        public override System.Threading.Tasks.Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(true);
        }

        public override System.Threading.Tasks.Task RemoveClaimAsync(User user, System.Security.Claims.Claim claim)
        {
            throw new NotImplementedException();
        }

        public override System.Threading.Tasks.Task RemoveRefreshToken(string hashedTokenId)
        {
            using (var session = DataStore.OpenSession())
            {
                var token = session.QueryOver<RefreshToken>().Where(r => r.Id == hashedTokenId).SingleOrDefault();
                session.Delete(token);
                session.Flush();
            }
            return Task.FromResult<RefreshToken>(null);
        }

        public override System.Threading.Tasks.Task SetEmailAsync(User user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public override System.Threading.Tasks.Task SetSecurityStampAsync(User user, string stamp)
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

        public override System.Threading.Tasks.Task UpdateUserAsync(User user)
        {
            using (var session = DataStore.OpenSession())
            {
                var dbUser = session.Get<User>(user.Id);
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
                DataService.SaveOrUpdate(session, dbUser);
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