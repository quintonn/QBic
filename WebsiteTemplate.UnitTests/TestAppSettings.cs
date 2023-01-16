using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Data;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.UnitTests
{
    internal class TestAppSettings : ApplicationSettingsCore
    {
        public override bool ShowSQL => false;

        public override DBProviderType DataProviderType => DBProviderType.SQLITE;

        public override string ApplicationPassPhrase => "1234567890";

        public override bool UpdateDatabase => true;

        public override Type GetApplicationStartupType => typeof(TestAppStartup);

        public override string SystemEmailAddress => "system@example.com";

        public override bool TokenEndpointAllowInsecureHttpRequests => false;

        public override IPersistenceConfigurer GetPersistenceConfigurer(string databaseName)
        {
            var connectionString = Config.GetConnectionString(databaseName);

            var currentDirectory = QBicUtils.GetCurrentDirectory();
            connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

            var configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            return configurer;
        }

        public override string GetApplicationName()
        {
            return "Test Application";
        }
    }
}
