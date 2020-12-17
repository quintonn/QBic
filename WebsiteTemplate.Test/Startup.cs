using DocumentGenerator.Styles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Qactus.Authorization.Core;
using Qactus.Authorization.Jwt;
using Qactus.Authorization.Jwt.Default;
using QBic.Core.Data;
using QBic.Core.Services;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using Unity;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.Services.Background;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Models;
using WebsiteTemplate.Test.SiteSpecific;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test
{
    public class Startup
    {
        private IServiceProvider Container { get; set; }
        private IConfiguration Config;

        public Startup(IConfiguration config)
        {
            Config = config;
        }

        private List<IJwtAuthenticationProvider> optionsProviders = new List<IJwtAuthenticationProvider>();

        public void ConfigureServices(IServiceCollection services)
        {
            //var Container = new UnityContainer();
            //Container.LoadConfiguration();

            var appSettings = new AppSettings();// Container.Resolve<ApplicationSettingsCore>();
            services.AddSingleton<ApplicationSettingsCore>(appSettings);

#if (DEBUG)
            if (appSettings.DebugStartup)
            {
                if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();
            }
#endif

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });

            var builder = services.AddMvcCore(x =>
            {
                x.EnableEndpointRouting = false; // to enable endpoint routing. I think that's to use routing attributes
            }).AddNewtonsoftJson(options =>
            {
                options.UseCamelCasing(true);
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            })
            .AddJsonOptions(options =>
            {
                
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            //.SerializerSettings.ContractResolver = new DefaultContractResolver());
            //.AddJsonOptions(options => options. .SerializerSettings.ContractResolver = new DefaultContractResolver());

            // Register and setup NHibernate
            var connectionString = Config.GetConnectionString("testconnection"); // the connection string can be up to the end user to provide, they can choose how to get it.
            var updateDB = true;
            //DataService.SetupQactusSessionFactory(services, connectionString, new NHibernate.Dialect.MsSql2012Dialect(), updateDB);

            // Regiser and setup all Authentication Server related stuff
            services.AddTransient<IRefreshToken, RefreshToken>();


            //services.AddSingleton<TestUser>(); // doesn't seem required
            //services.AddSingleton<WebsiteUser>();

            services.AddTransient<AuditService>();

            // registrations because Unity was removed and auto resolving doesn't work anymore
            services.AddTransient<UserProcessor>();
            services.AddTransient<UserRoleService>();
            services.AddTransient<UserRoleProcessor>();
            services.AddTransient<UserService>();
            services.AddTransient<MenuService>();
            services.AddTransient<MenuProcessor>();
            services.AddTransient<BackupService>();
            services.AddTransient<StyleSetup>();
            services.AddSingleton<BackgroundService>();
            services.AddTransient<BackgroundManager>();
            services.AddTransient<BackgroundWorker>();
            services.AddTransient<InitializationProcessor>();
            services.AddTransient<ApplicationService>();
            services.AddTransient<UserMenuProcessor>();
            services.AddTransient<ActionExecutionProcessor>();
            services.AddTransient<ViewMenuProcessor>();
            services.AddTransient<InputEventProcessor>();
            services.AddTransient<PropertyChangeProcessor>();
            services.AddTransient<PingProcessor>();
            services.AddSingleton<DataService>();
            services.AddTransient<InitializationProcessor, InitializationProcessor>();
            services.AddTransient<EventService>();
            services.AddTransient<FileProcessor>();
            services.AddTransient<UpdateViewProcessor>();

            services.AddTransient<BackupProcessor>();

            services.RegisterJwtUserProviders<User>(true);
            //services.RegisterJwtUserProviders<WebsiteUser>(false);

            //services.AddTransient<AuditService>();

            builder.AddAuthorization(); // This makes the AuthorizeAttribute work. Without this, requests are not authorized at all

            services.AddSingleton<IJwtAuthenticationProvider, TestJwtAuthProvider>();

            // if (!Providers.ContainsKey(services))
            {
                //Providers.Add(services, new List<IJwtAuthenticationProvider>());
                // this is also need for authentication to work, without this we get authenticationScheme errors
                services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Authentication needs a default scheme
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Authentication needs a default scheme
                }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    //var optionsProviders = Providers[services];

                    var tokenProviderParams = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = optionsProviders.Select(o => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(o.SecretKey))).ToList(),
                        ValidateIssuer = true,
                        ValidIssuers = optionsProviders.Select(o => o.Issuer).ToList(),
                        ValidateAudience = true,
                        ValidAudiences = optionsProviders.Select(o => o.Audience).ToList(),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    };

                    // Needed, else authentication does not work. Same as 100: below
                    //TODO: is there a way to get this without having to store it?
                    options.TokenValidationParameters = tokenProviderParams;
                });

                // This is required for authentication to work
                services.AddHttpContextAccessor();
            }

            var dataStore = DataStore.GetInstance(true, Config, services);
            services.AddSingleton(dataStore);// appSettings.UpdateDatabase));

            services.AddTransient<ApplicationStartup, AppStartup>();
            

            //services.AddScoped(typeof(ApplicationStartup), appSettings.GetApplicationStartupType);

            services.AddScoped<UserInjector, DefaultUserInjector>();

            //services.AddTransient(app.GetDataProtectionProvider());

            //var serviceProvider = services.BuildServiceProvider();
            //var appStartup = serviceProvider.GetService<ApplicationStartup>();
            //appStartup.RegisterUnityContainers(serviceProvider);
            //appSettings.PerformAdditionalStartupConfiguration(services);

            // End of auth stuff

            // Microsoft.AspNetCore.Mvc.Formatters.Json
            //builder.AddJsonFormatters();

            // Microsoft.Extensions.DependencyInjection
            //builder.AddNewtonsoftJson();

            //optionsProviders = serviceProvider.GetServices<IJwtAuthenticationProvider>().ToList();
            //Providers[services].AddRange(optionsProvider);
        }


        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.Use((context, next) =>
            {
                context.Request.EnableBuffering();
                return next();
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            //app.UseFileServer(true);


            optionsProviders = serviceProvider.GetServices<IJwtAuthenticationProvider>().ToList();
            var appStartup = serviceProvider.GetService<ApplicationStartup>();
            appStartup.RegisterUnityContainers(serviceProvider);
            //appSettings.PerformAdditionalStartupConfiguration(services);

            var appSettings = app.ApplicationServices.GetService<ApplicationSettingsCore>();

            //app.ApplicationServices.GetService<SystemLogger>().Setup(appSettings.LogLevel);
            new SystemLogger().Setup(appSettings.LogLevel);

            app.UseHttpsRedirection();
            //app.UsePathBase("/test");  // will add /test to all my routes, regardless of their paths

            app.UseMiddleware<JwtAuthenticationMiddleware>(); // this is for logging-in and getting new tokens
            app.UseMiddleware<JwtAuthorizationMiddleware>(); // this is for validating tokens when a called has the Authorize attribute

            app.UseDeveloperExceptionPage();

            // 100:
            // Needed to 'add' tokens to http context for verifying access.
            // Not needed when calling the login call
            // without this, tokens are validated, but the Authorize attribute doesn't use it and httpcontext identity is null
            app.UseAuthentication();

            //app.UseResponseCaching();
            app.UseResponseCompression();


            //app.UseAuthorization(); --> Does not seem to be needed

            app.UseMvc();
        }
    }
}