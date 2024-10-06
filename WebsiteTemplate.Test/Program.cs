using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace WebsiteTemplate.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            Console.WriteLine("Done");
        }

        static long MAX_REQUEST_BODY_BYTES = 250 * 1024 * 1024; // 100MB

        public static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = new WebHostBuilder();

            builder.UseKestrel(opt =>
            {
                opt.Limits.MaxRequestBodySize = MAX_REQUEST_BODY_BYTES;
            });

            builder.UseContentRoot(Directory.GetCurrentDirectory());
#if (DEBUG)
            builder.UseUrls("https://*:5001", "http://*:5000");
#endif
            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json", true, true);
            
            // Load environment app settings file
            var appFileName = "appsettings." + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + ".json";
            config.AddJsonFile(appFileName, true, true);

            builder.UseConfiguration(config.Build());

            builder.UseStartup<Startup>();

            return builder;
        }
    }
}
