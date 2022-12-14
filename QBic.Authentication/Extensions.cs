using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace QBic.Authentication
{
    public static class Extensions
    {
        public static IServiceCollection RegisterJwtUserProviders<USER>(this IServiceCollection services, bool makeDefaultIUserProvider = false) where USER : class, IUser
        {
            services.AddSingleton<ILookupNormalizer, DefaultLookupNormalizer>();
            services.AddSingleton<IdentityErrorDescriber>();

            /* This works, but what if I have multiple types of IUser */
            //services.AddTransient<IUser, USER>(); 
            //services.AddSingleton<IUserStore<IUser>, NHibernateUserStore<IUser>>(); //TODO

            //services.AddTransient<IPasswordHasher<IUser>, PasswordHasher<IUser>>();            
            //services.AddTransient<UserManager<IUser>, UserManager<IUser>>();

            if (makeDefaultIUserProvider == true)
            {
                // register the current USER type to be the default for IUser, if not specified
                services.AddTransient<IUserStore<IUser>, QBicUserStore<IUser>>();
                services.AddTransient<IUserPasswordStore<IUser>, QBicUserStore<IUser>>();
                services.AddTransient<IUserEmailStore<IUser>, QBicUserStore<IUser>>();
                services.AddTransient<IPasswordHasher<IUser>, PasswordHasher<IUser>>();
                services.AddTransient<UserManager<IUser>, UserManager<IUser>>();
            }

            // Register the specific USER type
            services.AddTransient<IUserStore<USER>, QBicUserStore<USER>>();
            services.AddTransient<IUserPasswordStore<USER>, QBicUserStore<USER>>();
            services.AddTransient<IUserEmailStore<USER>, QBicUserStore<USER>>();
            services.AddTransient<IPasswordHasher<USER>, PasswordHasher<USER>>();
            services.AddTransient<UserManager<USER>, UserManager<USER>>();

            /* This will allow multiple types of IUser, but require those specific registrations */
            //services.AddSingleton<IUserStore<NH_User>, NHibernateUserStore<NH_User>>();
            //services.AddSingleton<IPasswordHasher<NH_User>, PasswordHasher<NH_User>>();
            //services.AddSingleton<UserManager<NH_User>>();

            return services;
        }
    }
}
