﻿using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Processing
{
    public class UserMenuProcessor : CoreProcessor<IList<Menu>>
    {
        public UserMenuProcessor(IServiceProvider container, ILogger<UserMenuProcessor> logger)
            : base(container, logger)
        {

        }

        public async override System.Threading.Tasks.Task<IList<Menu>> ProcessEvent(int eventId)
        {
            var results = new List<Menu>();
            var user = await GetLoggedInUser();
            if (user == null)
            {
                return new List<Menu>(); // happens after failed Backup Restore
            }
            using (var session = DataService.OpenSession())
            {
                var events = GetAllowedEventsForUser(user.Id).ToArray();

                var mainMenus = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.In("Event", events))
                                           .Add(Restrictions.IsNull("ParentMenu"))
                                           .List<Menu>()
                                           .ToList();
                
                mainMenus.ForEach(results.Add);

                var mainMenusWithSubMenus = session.CreateCriteria<Menu>()
                                           .Add(Restrictions.IsNull("Event"))
                                           .Add(Restrictions.IsNull("ParentMenu"))
                                           .List<Menu>()
                                           .ToList();
                mainMenusWithSubMenus.ForEach(m =>
                {
                    m.ParentMenu = null;
                    AddSubMenu(m, session, events);
                    if (m.SubMenus.Count > 0)
                    {
                        results.Add(m);
                    }
                });
            }

            return results.OrderBy(m => m.Position).ToList();
        }

        private void AddSubMenu(Menu menu, ISession session, int[] events)
        {
            menu.ParentMenu = null;
            var subMenus = session.CreateCriteria<Menu>()
                                  .CreateAlias("ParentMenu", "parent")
                                  .Add(Restrictions.Or(Restrictions.In("Event", events), Restrictions.IsNull("Event")))
                                  .Add(Restrictions.Eq("parent.Id", menu.Id))
                                  .List<Menu>()
                                  .ToList();
            foreach (var subMenu in subMenus)
            {
                AddSubMenu(subMenu, session, events);
            }
            menu.SubMenus = subMenus.OrderBy(m => m.Position).ToList();
        }
    }
}