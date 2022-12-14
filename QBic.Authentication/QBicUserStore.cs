using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QBic.Authentication
{
    public class QBicUserStore<USER> : IUserStore<USER>, IUserPasswordStore<USER>, IUserEmailStore<USER>
                                             where USER : class, IUser
    {
        private ISessionFactory SessionFactory { get; set; }
        public QBicUserStore(IServiceProvider serviceProvider)
        {
            SessionFactory = serviceProvider.GetService<ISessionFactory>();
        }
        #region IUserStore methods

        public virtual async Task<IdentityResult> CreateAsync(USER user, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                await session.SaveOrUpdateAsync(user, cancellationToken);
                await session.FlushAsync();
            }
            return IdentityResult.Success;
        }

        public virtual Task<IdentityResult> DeleteAsync(USER user, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                session.Delete(session.Get<USER>(user?.Id));
                session.Flush();
            }
            
            return Task.FromResult(IdentityResult.Success);
        }

        public virtual Task<USER> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var user = session.Get<USER>(userId);
                return Task.FromResult(user);
            }
        }

        public virtual Task<USER> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                //var user = SessionFactory.RetrieveByKeyValues(Data.Core.SessionFactorysitory.QueryJoinType.And, ("UserName", normalizedUserName, QueryMatchType.Equals)).SingleOrDefault();
                var user = session.QueryOver<USER>().WhereRestrictionOn(x => x.UserName).IsInsensitiveLike(normalizedUserName).SingleOrDefault();
                return Task.FromResult(user);
            }
        }

        public virtual Task<string> GetNormalizedUserNameAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.UserName);//?.ToLower());
        }

        public virtual Task<string> GetUserIdAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public virtual Task<string> GetUserNameAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public virtual Task SetNormalizedUserNameAsync(USER user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UserName = normalizedName;
            return Task.FromResult(0);
        }

        public virtual Task SetUserNameAsync(USER user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public virtual Task<IdentityResult> UpdateAsync(USER user, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(IdentityResult.Success);
        }

        #endregion

        #region IUserPasswordStore methods
        public virtual Task SetPasswordHashAsync(USER user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public virtual Task<string> GetPasswordHashAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public virtual Task<bool> HasPasswordAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IUserEmailStore methods

        public virtual Task SetEmailAsync(USER user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public virtual Task<string> GetEmailAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email);
        }

        public virtual Task<bool> GetEmailConfirmedAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.EmailConfirmed == true);
        }

        public virtual Task SetEmailConfirmedAsync(USER user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public virtual Task<USER> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var user = session.QueryOver<USER>().WhereRestrictionOn(x => x.Email).IsInsensitiveLike(normalizedEmail).List().ToList().FirstOrDefault(); // password options will/should prevent duplicates 
                return Task.FromResult(user);
            }
        }

        public virtual Task<string> GetNormalizedEmailAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email?.ToLower());
        }

        public virtual Task SetNormalizedEmailAsync(USER user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.Email = normalizedEmail;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        #endregion

        public void Dispose()
        {
        }
    }
}
