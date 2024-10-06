using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using NHibernate;
using QBic.Core.Auth;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using WebsiteTemplate.Models.NonDatabase;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Test.SiteSpecific
{
    public class AppSettings : ApplicationSettingsCore
    {
        public override bool EnableAuditing => true; // Will audit everything

        public override bool DebugStartup => false; //  will create a window to allow debugging when starting if true

        public override bool UpdateDatabase => true; // Set to true first time to create tables. Also set to true after making changes

        public override string ApplicationPassPhrase => "xxxxxxxxxxxxxxxxqqqqqqqqqqqqqqqq"; // must be at least 32 characters long (multiples of 8)

        public override bool ShowSQL => false;

        //public override TimeSpan AccessTokenExpireTimeSpan => TimeSpan.FromSeconds(25);
        public override bool DebugUserEvents => true;
        public override string SystemEmailAddress
        {
            get
            {
                return "q10atwork@gmail.com";
            }
        }

        public override bool TokenEndpointAllowInsecureHttpRequests => false;
        public override bool UseHttpsRedirection => true;

        public override string GetApplicationName()
        {
            return "QBic";
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

            var temp = Config.GetSection("ConnectionStrings");
            var connectionString = Config.GetConnectionString(databaseName);

            //var connectionString = "Integrated Security=SSPI;Persist Security Info=False;Data Source=localhost;Initial Catalog=WebTest;MultipleActiveResultSets=true";
            var configurer = MsSqlConfiguration.MsSql2012.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            //var connectionString = "Data Source=##CurrentDirectory##\\Data\\test.db;Version=3;Journal Mode=Off;Connection Timeout=12000";
            //var currentDirectory = QBicUtils.GetCurrentDirectory();
            //connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string
            //var configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);

            return configurer;
        }

        public override DBProviderType DataProviderType => DBProviderType.MSSQL;

        //public override IAuthConfig AuthConfig => new OidcAuth<OidcAuthResolver>()
        //{
        //    ClientId = "286831252286275587",
        //    Authority = "http://localhost:5050",
        //    RedirectUrl = "http://localhost:1234/",
        //    Scope = "openid profile email",
        //    ResponseType = "code"
        //};

        public override bool EnableGoogleAutoBackups => false;
        public override GoogleBackupConfig GoogleBackupConfig => new GoogleBackupConfig()
        {
            //DailyRunTimeUTC = new TimeOnly(22, 00) // midnight South African time
            DailyRunTimeUTC = new TimeOnly(15, 00) // 4pm UK time
        };
    }
}