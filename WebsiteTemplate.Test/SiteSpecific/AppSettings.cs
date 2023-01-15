using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Models.NonDatabase;
using WebsiteTemplate.Test.MenuItems.Users;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.SiteSpecific
{
    public class AppSettings : ApplicationSettingsCore
    {
        public override bool EnableAuditing => true; // Will audit everything

        public override bool DebugStartup => false; //  will create a window to allow debugging when starting if true

        public override bool UpdateDatabase => true; // Set to true first time to create tables. Also set to true after making changes

        public override string ApplicationPassPhrase => "xxxxxxxxxxxxxxxx";

        public override bool ShowSQL => true;

        public override Type GetApplicationStartupType
        {
            get
            {
                return typeof(Startup);
            }
        }

        public override string SystemEmailAddress
        {
            get
            {
                return "q10atwork@gmail.com";
            }
        }

        public override bool TokenEndpointAllowInsecureHttpRequests => false;

        public override string GetApplicationName()
        {
            return "QBic";
        }

        public override void PerformAdditionalStartupConfiguration(IServiceCollection services)
        {
            services.AddTransient<UserInjector, TestUserInjector>();
        }

        public override List<SystemSettingItem> GetAdditionalSystemSettings(ISession session)
        {
            return new List<SystemSettingItem>()
            {
                new SystemSettingItem("SystemEmail", "System Email", Menus.InputItems.InputType.Text, true, "Email Settings", ""),
                new SystemSettingItem("TestCheck", "Test Check", Menus.InputItems.InputType.Boolean, true, "Email Settings", false),
            };
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(string databaseName)
        {
            //var connectionString = Config.GetConnectionString(databaseName);

            //var currentDirectory = QBicUtils.GetCurrentDirectory();
            //connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string

            //var configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            var connectionString = "Integrated Security=SSPI;Persist Security Info=False;Data Source=localhost;Initial Catalog=WebTest;MultipleActiveResultSets=true";
            var configurer = MsSqlConfiguration.MsSql2012.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            return configurer;
        }

        public override DBProviderType DataProviderType => DBProviderType.MSSQL;
    }
}