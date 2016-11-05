using System;
using System.Collections.Generic;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.SiteSpecific.DefaultsForTest
{
    public class DefaultApplicationSettings : ApplicationSettingsCore
    {
        public override string GetApplicationName()
        {
            return "Website Template";
        }

        public override void ConfigureSiteSpecificTypes(Dictionary<int, Type> systemTypes)
        {
            Console.WriteLine("");

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
    }
}