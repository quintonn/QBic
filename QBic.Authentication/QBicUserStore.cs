﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using System;
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

        public async Task<IdentityResult> CreateAsync(USER user, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                await session.SaveOrUpdateAsync(user, cancellationToken);
                await session.FlushAsync();
            }
            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(USER user, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                session.Delete(session.Get<USER>(user?.Id));
                session.Flush();
            }
            
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<USER> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var user = session.Get<USER>(userId);
                return Task.FromResult(user);
            }
        }

        public Task<USER> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using (var session = SessionFactory.OpenSession())
            {
                //var user = SessionFactory.RetrieveByKeyValues(Data.Core.SessionFactorysitory.QueryJoinType.And, ("UserName", normalizedUserName, QueryMatchType.Equals)).SingleOrDefault();
                var user = session.QueryOver<USER>().WhereRestrictionOn(x => x.UserName).IsInsensitiveLike(normalizedUserName).SingleOrDefault();
                return Task.FromResult(user);
            }
        }

        public Task<string> GetNormalizedUserNameAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.UserName);//?.ToLower());
        }

        public Task<string> GetUserIdAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(USER user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UserName = normalizedName;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(USER user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public Task<IdentityResult> UpdateAsync(USER user, CancellationToken cancellationToken)
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
        public Task SetPasswordHashAsync(USER user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IUserEmailStore methods

        public Task SetEmailAsync(USER user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.EmailConfirmed == true);
        }

        public Task SetEmailConfirmedAsync(USER user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            using (var session = SessionFactory.OpenSession())
            {
                session.SaveOrUpdate(user);
                session.Flush();
            }
            return Task.FromResult(0);
        }

        public Task<USER> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            //var user = SessionFactory.RetrieveByKeyValues(Data.Core.SessionFactorysitory.QueryJoinType.And, ("Email", normalizedEmail, QueryMatchType.Equals)).SingleOrDefault();
            using (var session = SessionFactory.OpenSession())
            {
                var user = session.QueryOver<USER>().WhereRestrictionOn(x => x.Email).IsInsensitiveLike(normalizedEmail).SingleOrDefault();
                return Task.FromResult(user);
            }
        }

        public Task<string> GetNormalizedEmailAsync(USER user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user?.Email?.ToLower());
        }

        public Task SetNormalizedEmailAsync(USER user, string normalizedEmail, CancellationToken cancellationToken)
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
