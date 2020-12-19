using log4net.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QBic.Core.Utilities;
using System;
using System.IO;

namespace WebsiteTemplate.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new SystemLogger().Setup(Level.All);
            SystemLogger.GetLogger<Program>().Info("Starting...");
            CreateHostBuilder(args).Build().Run();

            Console.WriteLine("Done");
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            SystemLogger.GetLogger<Program>().Info("Starting...");
            var builder = new WebHostBuilder();

            builder.UseIISIntegration();
            builder.UseKestrel();

            //builder.UseKestrel((hostingContext, options) =>
            // {
            //     var httpPort = 80;
            //     var httpsPort = 443;
            //     if (hostingContext.HostingEnvironment.IsDevelopment() || true)
            //     {
            //         httpPort = 5000;
            //         httpsPort = 5001;
            //     }
            //     SystemLogger.GetLogger<Program>().Info("port = " + httpPort + " & " + httpsPort);
            //     // listen for HTTP
            //     options.ListenAnyIP(httpPort);
            //     //options.ListenLocalhost(5000);

            //     // retrieve certificate from store
            //     using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            //     {
            //         store.Open(OpenFlags.ReadOnly);
            //         var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
            //         if (certs.Count > 0)
            //         {
            //             var certificate = certs[0];

            //             // listen for HTTPS
            //             options.ListenAnyIP(httpsPort, listenOptions =>
            //             {
            //                 listenOptions.UseHttps(certificate);
            //             });
            //         }
            //     }
            // });


            builder.UseContentRoot(Directory.GetCurrentDirectory());
#if (DEBUG)
            builder.UseUrls("https://*:5001", "http://*:5000");
#endif
            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json", true, true);
            builder.UseConfiguration(config.Build());

            builder.ConfigureLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);

                x.ClearProviders();
                x.AddDebug();
                x.AddConsole();
            });
            builder.UseStartup<Startup>();

            return builder;
        }
    }
}
