using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.SiteSpecific.Utilities;

namespace ImplementationTest.CustomMenuItems
{
    public class TestApplicationSettings : IApplicationSettings
    {
        public string GetApplicationName()
        {
            return "Implementation Test";
        }


        public void RegisterUnityContainers(IUnityContainer container)
        {
            
        }
    }
}