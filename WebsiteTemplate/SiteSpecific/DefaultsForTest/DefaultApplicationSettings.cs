using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace WebsiteTemplate.SiteSpecific.DefaultsForTest
{
    public class DefaultApplicationSettings : IApplicationSettings
    {
        public string GetApplicationName()
        {
            return "Website Template";
        }

        public void RegisterUnityContainers(IUnityContainer container)
        {
            
        }
    }
}