using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;

namespace QBic.Authentication
{
    public abstract class JwtAuthenticationProvider<TOKEN, USER> : IJwtAuthenticationProvider where TOKEN : class, IRefreshToken
                                                                                         where USER : class, IUser
    {
        private Func<TOKEN> TokenCreator { get; set; }

        UserManager<USER> UserManager;
        private ISessionFactory SessionFactory { get; set; }

        public JwtAuthenticationProvider(IServiceProvider serviceProvider)
        {
            UserManager = serviceProvider.GetService<UserManager<USER>>();
            SessionFactory = serviceProvider.GetService<ISessionFactory>();
            TokenCreator = () => serviceProvider.GetService<TOKEN>();
        }

        public virtual string Path => "/token";

        public virtual bool AllowInsecureHttp => false;

        public abstract string ClientId { get; }

        public abstract string Issuer { get; }

        public abstract string Audience { get; }

        public virtual TimeSpan AccessTokenExpiration => TimeSpan.FromMinutes(5);

        public virtual TimeSpan RefreshTokenExpiration => TimeSpan.FromDays(1);

        public abstract string SecretKey { get; }

        public virtual string SigningAlgorithm => SecurityAlgorithms.HmacSha256;

        public virtual Func<string> NonceGenerator => () => Guid.NewGuid().ToString();  

        public virtual async Task DeleteRefreshToken(string token)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var dbToken = await session.QueryOver<IRefreshToken>().Where(x => x.Token == token).SingleOrDefaultAsync();
                if (dbToken != null)
                {
                    await session.DeleteAsync(dbToken);
                    session.Flush();
                }
            }
        }

        public virtual async Task<bool> FindRefreshToken(string token)
        {
            using (var session = SessionFactory.OpenSession())
            {
                return await session.QueryOver<IRefreshToken>().Where(x => x.Token == token).RowCountAsync() > 0;
            }
        }

        public virtual Task<IEnumerable<Claim>> GetAdditionalClaims(string username)
        {
            return Task.FromResult< IEnumerable<Claim>>(new List<Claim>());
        }

        public virtual async Task StoreNewRefreshTokenAsync(string token)
        {
            using (var session = SessionFactory.OpenSession())
            {
                var dbToken = TokenCreator?.Invoke();
                dbToken.Token = token;
                await session.SaveOrUpdateAsync(dbToken);
                session.Flush();
            }
        }

        public virtual async Task<bool> VerifyPassword(string username, string password)
        {
            var user = await UserManager.FindByNameAsync(username);
            if (user == null)
            {
                user = await UserManager.FindByEmailAsync(username);
            }

            var res = await UserManager.CheckPasswordAsync(user, password);
            return res;
        }

        public virtual Task<VerificationResult> VerifyUserCanLogin(string username)
        {
            return Task.FromResult(VerificationResult.Success());
        }
    }
}
