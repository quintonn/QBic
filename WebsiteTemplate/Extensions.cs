using DocumentGenerator.Styles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QBic.Authentication;
using QBic.Core.Data;
using QBic.Core.Models;
using QBic.Core.Services;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using WebsiteTemplate.Backend.Processing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.Services.Background;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Models;
using WebsiteTemplate.Security;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate
{
    public static class Extensions
    {
        private static IList<IJwtAuthenticationProvider> OptionsProviders { get; set; }
        private static bool ConfigureServicesCalled = false;
        private static bool ConfigureCalled = false;
        //AppStartup
        public static IServiceCollection UseQBic<T, A>(this IServiceCollection services, IConfiguration configuration, Action<IdentityOptions> identityOptions = null) where T : ApplicationSettingsCore
            where A : ApplicationStartup
        {
            services.AddTransient<ApplicationStartup, A>();
            return services.UseQBic<T>(configuration, identityOptions);
        }
        public static IServiceCollection UseQBic<T>(this IServiceCollection services, IConfiguration configuration, Action<IdentityOptions> identityOptions = null) where T: ApplicationSettingsCore
        {
            ConfigureServicesCalled = true;
            services.AddSingleton<ApplicationSettingsCore, T>();

            var appSettings = Activator.CreateInstance<T>();

            if (appSettings.ApplicationPassPhrase.Length < 32)
            {
                throw new Exception("Application Pass Phrase must be at least 32 characters long to be used by JWT Encryption");
            }

            appSettings.SetConfig(configuration);

            services.AddSingleton<IApplicationSettings>(appSettings);

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
            services.AddTransient<ViewDetailProcessing>();

            services.AddTransient<IRefreshToken, RefreshToken>();

            services.RegisterJwtUserProviders<User>(true);

            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
                // Form key length limit 2048 exceeded
                options.KeyLengthLimit = int.MaxValue;   // i think my data is being sent in the key
                options.ValueLengthLimit = int.MaxValue; // but without this large files fail too.
            });

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
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //options.SerializerSettings.TypeNameHandling = includeTypeInfo == true ? TypeNameHandling.All : TypeNameHandling.None;
                options.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
            })
            .AddJsonOptions(options =>
            {

                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            builder.AddAuthorization(); // This makes the AuthorizeAttribute work. Without this, requests are not authorized at all
            services.AddSingleton<IJwtAuthenticationProvider, QBicJwtAuthProvider>();

            var dataStore = DataStore.GetInstance(appSettings.UpdateDatabase, appSettings, configuration, services);
            services.AddSingleton(dataStore);// appSettings.UpdateDatabase));

            // This is required for authentication to work
            services.AddHttpContextAccessor();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Authentication needs a default scheme
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Authentication needs a default scheme
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                if (ConfigureCalled == false)
                {
                    Console.WriteLine("Call app.UseQBic(IServiceProvider); from your Startup Configure(IApplicationBuilder app, IServiceProvider serviceProvider) method.");
                    SystemLogger.GetLogger(typeof(Extensions)).LogError("Call app.UseQBic(IServiceProvider); from your Startup Configure(IApplicationBuilder app, IServiceProvider serviceProvider) method.");
                    throw new Exception("Call app.UseQBic(IServiceProvider); from your Startup Configure(IApplicationBuilder app, IServiceProvider serviceProvider) method.");
                }

                var tokenProviderParams = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = OptionsProviders.Select(o => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(o.SecretKey))).ToList(),
                    ValidateIssuer = true,
                    ValidIssuers = OptionsProviders.Select(o => o.Issuer).ToList(),
                    ValidateAudience = true,
                    ValidAudiences = OptionsProviders.Select(o => o.Audience).ToList(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                // Needed, else authentication does not work. Same as 100: below
                //TODO: is there a way to get this without having to store it?
                options.TokenValidationParameters = tokenProviderParams;
            });

            services.AddScoped<UserInjector, DefaultUserInjector>();

            if (identityOptions != null)
            {
                services.AddIdentityCore<User>(identityOptions).AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);
            }
            else
            {
                services.AddIdentityCore<User>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 0;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedAccount = true;
                }).AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);
            }

            return services;
        }

        public static IApplicationBuilder UseQBic(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            if (ConfigureServicesCalled == false)
            {
                Console.WriteLine("Call services.UseQBic<AppSettings, AppStartup>(IConfiguration) from your Startup ConfigureServices(IServiceCollection services) method.");
                SystemLogger.GetLogger(typeof(Extensions)).LogError("Call services.UseQBic<AppSettings, AppStartup>(IConfiguration) from your Startup ConfigureServices(IServiceCollection services) method.");
                throw new Exception("Call services.UseQBic<AppSettings, AppStartup>(IConfiguration) from your Startup ConfigureServices(IServiceCollection services) method.");
            }
            ConfigureCalled = true;
            app.Use((context, next) =>
            {
                // needed to read request body in controllers
                context.Request.EnableBuffering();
                return next();
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "web-files")),
            //    ServeUnknownFileTypes = true // else I get: The request path /api/v1/initializeSystem does not match a supported file type
            //});

            var appStartup = serviceProvider.GetService<ApplicationStartup>();
            appStartup?.RegisterUnityContainers(serviceProvider);
            //appSettings.PerformAdditionalStartupConfiguration(services);

            var appSettings = app.ApplicationServices.GetService<ApplicationSettingsCore>();

            //app.ApplicationServices.GetService<SystemLogger>().Setup(appSettings.LogLevel);
            //new SystemLogger().Setup(appSettings.LogLevel);

            //app.UseHttpsRedirection();
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

            OptionsProviders = serviceProvider.GetServices<IJwtAuthenticationProvider>().ToList();

            return app;
        }
    }
}
