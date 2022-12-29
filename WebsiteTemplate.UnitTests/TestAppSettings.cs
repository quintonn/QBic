using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System.Data;

namespace WebsiteTemplate.UnitTests
{
    internal class TestAppSettings : IApplicationSettings
    {
        public bool ShowSQL => false;

        private IConfiguration Config { get; set; }

        public DBProviderType DataProviderType => DBProviderType.SQLITE;

        public TestAppSettings(IConfiguration config)
        {
            Config = config;
        }

        public IPersistenceConfigurer GetPersistenceConfigurer(string databaseName)
        {
            var connectionString = Config.GetConnectionString(databaseName);

            var currentDirectory = QBicUtils.GetCurrentDirectory();
            connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

            var configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            return configurer;
        }
    }
}
