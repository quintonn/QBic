using Microsoft.Practices.Unity;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Data;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
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

        public void SetupDefaults(ISession session)
        {
            var testMenuList = session.CreateCriteria<Menu>()
                                              .Add(Restrictions.Eq("Name", "Test1"))
                                              .List<Menu>();
            if (testMenuList.Count == 0)
            {
                var testMenu = new Menu()
                {
                    Name = "Test1",
                };
                session.Save(testMenu);

                var testMenuList2 = session.CreateCriteria<Menu>()
                                   .Add(Restrictions.Eq("Name", "Test2"))
                                   .List<Menu>();
                if (testMenuList2.Count == 0)
                {
                    var testMenu2 = new Menu()
                    {
                        Name = "Test2",
                        ParentMenu = testMenu,
                    };
                    session.Save(testMenu2);

                    var menu3 = new Menu()
                    {
                        Name = "Test3",
                        ParentMenu = testMenu2,
                        Event = EventNumber.ViewMenus
                    };

                    session.Save(menu3);
                }
            }
        }
    }
}