using Microsoft.Owin.Security.DataProtection;
using Microsoft.Practices.Unity;
using Owin;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Utilities;

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

        public override void ConfigureSiteSpecificTypes(Dictionary<int, Type> systemTypes)
        {

        }

        public override string GetApplicationName()
        {
            return "Web Template Test";
        }

        public override void PerformAdditionalStartupConfiguration(IAppBuilder app, IUnityContainer container)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false) System.Diagnostics.Debugger.Launch();

            container.RegisterInstance(app.GetDataProtectionProvider());
        }
    }
}