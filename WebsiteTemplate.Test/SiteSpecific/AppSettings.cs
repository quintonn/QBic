using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using System;
using System.Collections.Generic;
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

        public override string ApplicationPassPhrase => "8375743900958305380983509";

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

        public override bool TokenEndpointAllowInsecureHttpRequests => true;

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
    }
}