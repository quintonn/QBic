using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.IO;

namespace WebsiteTemplate.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TestSettingUpStuffWithoutWebServer();
            CreateHostBuilder(args).Build().Run();

            Console.WriteLine("Done");
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new WebHostBuilder();

            //builder.UseIISIntegration();
            builder.UseKestrel();
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.UseUrls("https://localhost:5001", "http://localhost:5000");
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
            //builder.UseStaticWebAssets();

            return builder;
        }
    }
}
