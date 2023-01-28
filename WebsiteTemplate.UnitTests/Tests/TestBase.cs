using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;
using WebsiteTemplate.UnitTests.Models;
using WebsiteTemplate.UnitTests.Setup;

namespace WebsiteTemplate.UnitTests.Tests
{
    internal abstract class TestBase
    {
        protected IConfiguration Config { get; set; }
        protected DataService DataService { get; set; }
        protected ServiceProvider ServiceProvider { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            DataStore.ClearInstance();
            var types = typeof(User).Assembly.GetTypes().ToList(); // force WebsiteTemplate types to load for datastore initialization
            Console.WriteLine(types.Count);

            types = typeof(Employee).Assembly.GetTypes().ToList(); // force WebsiteTemplate types to load for datastore initialization
            Console.WriteLine(types.Count);

            Config = new ConfigurationBuilder().Build();

            var serviceCollection = new ServiceCollection()
            .AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Information);
            })
            .AddSingleton<IConfiguration>(Config);


            serviceCollection.UseQBic<TestAppSettings, TestAppStartup>(Config);

            ServiceProvider = serviceCollection.BuildServiceProvider();
            var factory = ServiceProvider.GetService<ILoggerFactory>();
            SystemLogger.Setup(factory);

            DataService = ServiceProvider.GetService<DataService>();
        }
    }
}
