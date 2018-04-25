using Microsoft.Owin.Security.DataProtection;
using Unity;
using Owin;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Utilities;
using WebsiteTemplate.Models.NonDatabase;
using WebsiteTemplate.Backend.Users;
using WebsiteTemplate.Test.MenuItems.Users;
using log4net.Core;

namespace WebsiteTemplate.Test.SiteSpecific
{
    public class AppSettings : ApplicationSettingsCore
    {
        public override string ApplicationPassPhrase
        {
            get
            {
                return "8375743900958305380983509";
            }
        }

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

        public override string GetApplicationName()
        {
            return "Web Template Test";
        }

        public override void PerformAdditionalStartupConfiguration(IAppBuilder app, IUnityContainer container)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();

            container.RegisterInstance(app.GetDataProtectionProvider());

            container.RegisterType<UserInjector, TestUserInjector>();
        }

        public override List<SystemSettingItem> GetAdditionalSystemSettings()
        {
            return new List<SystemSettingItem>()
            {
                new SystemSettingItem("SystemEmail", "System Email", Menus.InputItems.InputType.Text, true, "Email Settings", ""),
                new SystemSettingItem("TestCheck", "Test Check", Menus.InputItems.InputType.Boolean, true, "Email Settings", false),
            };
        }

        public override bool DebugStartup
        {
            get
            {
                return true;
            }
        }

        public override Level LogLevel
        {
            get
            {
                return Level.Debug;
            }
        }

        public override bool EnableAuditing
        {
            get
            {
                return true;
            }
        }

        public override bool UpdateDatabase
        {
            get
            {
                return true;
            }
        }
    }
}