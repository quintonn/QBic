﻿using FluentNHibernate.Cfg.Db;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System.Data;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.UnitTests.Setup
{
    internal class TestAppSettings : ApplicationSettingsCore
    {
        public override bool ShowSQL => false;

        public override DBProviderType DataProviderType => DBProviderType.SQLITE;

        public override string ApplicationPassPhrase => "12345678901234567890123456789123";

        public override bool UpdateDatabase => true;

        public override string SystemEmailAddress => "system@example.com";

        public override bool TokenEndpointAllowInsecureHttpRequests => false;

        string name = QBicUtils.CreateNewGuid();
        public override IPersistenceConfigurer GetPersistenceConfigurer(string databaseName)
        {
            var connectionString = $"Data Source=file:{name}?mode=memory&cache=shared;Version=3;New=True";

            var configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            return configurer;
        }

        public override string GetApplicationName()
        {
            return "Test Application";
        }
    }
}
