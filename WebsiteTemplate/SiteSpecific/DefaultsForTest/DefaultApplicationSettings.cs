using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Data;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.SiteSpecific.DefaultsForTest
{
    public class DefaultApplicationSettings : ApplicationSettingsCore
    {
        public override string GetApplicationName()
        {
            return "Website Template";
        }

        public override string ApplicationPassPhrase
        {
            get
            {
                return "||22^master^JOIN^continue^12||";
            }
        }

        public override Type GetApplicationStartupType
        {
            get
            {
                return typeof(DefaultStartup);
            }
        }

        public override string SystemEmailAddress
        {
            get
            {
                return "q10atwork@gmail.com";
            }
        }

        public override bool TokenEndpointAllowInsecureHttpRequests => true;

        public override DBProviderType DataProviderType => DBProviderType.SQLITE;

        public override IPersistenceConfigurer GetPersistenceConfigurer(string databaseName)
        {
            var connectionString = Config.GetConnectionString(databaseName);
            
            var currentDirectory = QBicUtils.GetCurrentDirectory();
            connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

            var configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            return configurer;
        }
    }
}