using Microsoft.Practices.Unity;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Processing
{
    public class UserMenuProcessor : CoreProcessor<IList<Menu>>
    {
        public UserMenuProcessor(IUnityContainer container)
            : base(container)
        {

        }

        public async override System.Threading.Tasks.Task<IList<Menu>> ProcessEvent(int eventId)
        {
            var results = new List<Menu>();
            var user = await BasicAuthentication.ControllerHelpers.Methods.GetLoggedInUserAsync() as User;
            using (var session = Store.OpenSession())
            {
                var events = GetAllowedEventsForUser(session, user.Id).ToArray();

                var mainMenus = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.In("Event", events))
                                           .Add(Restrictions.IsNull("ParentMenu"))
                                           .List<Menu>()
                                           .ToList();
                mainMenus.ForEach(m =>
                {
                    results.Add(m);
                });

                var mainMenusWithSubMenus = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.IsNull("Event"))
                                           .Add(Restrictions.IsNull("ParentMenu"))
                                           .List<Menu>()
                                           .ToList();
                mainMenusWithSubMenus.ForEach(m =>
                {
                    m.ParentMenu = null;
                    AddSubMenu(m, session);
                    results.Add(m);
                });
            }
            return results;
        }

        private void AddSubMenu(Menu menu, ISession session)
        {
            menu.ParentMenu = null;
            var subMenus = session.CreateCriteria<Menu>()
                                  .CreateAlias("ParentMenu", "parent")
                                  .Add(Restrictions.Eq("parent.Id", menu.Id))
                                  .List<Menu>()
                                  .ToList();
            foreach (var subMenu in subMenus)
            {
                AddSubMenu(subMenu, session);
            }
            menu.SubMenus = subMenus;
        }
    }
}