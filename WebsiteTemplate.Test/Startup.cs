using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QBic.Authentication;
using QBic.Core.Auth;
using QBic.Core.Utilities;
using System;
using System.Net;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Test.MenuItems.Users;
using WebsiteTemplate.Test.Models;
using WebsiteTemplate.Test.SiteSpecific;

namespace WebsiteTemplate.Test
{
    public class Startup
    {
        private IConfiguration Config;

        private static int DEFAULT_CACHE_TIME = 60 * 60 * 24 * 10;// 10 days

        public Startup(IConfiguration config)
        {
            Config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // This is needed if hosting on Linux environment such as azure
            services.AddLogging(x =>
            {
                x.AddConsole();
                x.AddDebug();

            });

            var idOptions = new Action<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;

                options.User.AllowedUserNameCharacters += " ";
            });
            services.UseQBic<AppSettings, AppStartup>(Config, idOptions);

            var appSettings = Activator.CreateInstance<AppSettings>();

            // Configure Auth
            if (appSettings.AuthConfig.AuthType == AuthType.Oidc)
            {
                var oidcConfig = appSettings.AuthConfig as IOidcAuth;
                services.AddAuthentication(x =>
                {
                    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.Authority = oidcConfig.Authority;
                    x.Audience = oidcConfig.ClientId;
                    x.IncludeErrorDetails = true;
                    x.RequireHttpsMetadata = false;
                    x.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidAudience = oidcConfig.ClientId,
                        ValidIssuer = oidcConfig.Authority,
                        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    };
                });
            }

            //services.AddScoped<UserInjector, DefaultUserInjector>(); // can i override the default one?
            services.AddScoped<UserInjector, TestUserInjector>();
            //User can override UserInjector with their own injector class

            // Add additional user authentication
            //services.AddSingleton<IJwtAuthenticationProvider, MobileJwtAuthProvider>();
            services.AddSingleton<IJwtAuthenticationProvider, MobileJwtAuthProvider>();
            // would be nice if this did work instead of the many lines below
            //services.AddSingleton<QBicUserStore<MobileUser>, MobileUserStore>();
            //services.RegisterJwtUserProviders<MobileUser>(false);
            services.AddTransient<IUserStore<MobileUser>, MobileUserStore>();
            services.AddTransient<IUserPasswordStore<MobileUser>, MobileUserStore>();
            services.AddTransient<IUserEmailStore<MobileUser>, MobileUserStore>();
            services.AddTransient<IPasswordHasher<MobileUser>, PasswordHasher<MobileUser>>();
            services.AddTransient<UserManager<MobileUser>, UserManager<MobileUser>>();

            services.AddMvc(options =>
            {
                options.CacheProfiles.Add("DefaultImageCache",
                    new CacheProfile()
                    {
                        Duration = DEFAULT_CACHE_TIME,
                    });
            });

            services.AddCors(o => o.AddPolicy("Default", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
                builder.WithHeaders("filename").WithExposedHeaders("filename");
            }));
            
            if (appSettings.UseHttpsRedirection)
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
                    options.HttpsPort = 5001;
                });
            }
        }


        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, ILoggerFactory logFactory)
        {
            SystemLogger.Setup(logFactory);
            logFactory.AddFile("Logs/log-{Date}.txt");
            app.UseCors("Default");
            app.UseQBic(serviceProvider);
        }
    }
}