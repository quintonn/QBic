﻿using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class MenuService
    {
        private EventService EventService { get; set; }
        private DataService DataService { get; set; }

        public MenuService(EventService eventService, DataService dataService)
        {
            DataService = dataService;
            EventService = eventService;
        }

        public Menu RetrieveMenuWithId(string menuId)
        {
            using (var session = DataService.OpenSession())
            {
                return session.Get<Menu>(menuId);
            }
        }

        public bool IsParentMenu(string menuId)
        {
            using (var session = DataService.OpenSession())
            {
                var childMenuItems = session.CreateCriteria<Menu>()
                                            .CreateAlias("ParentMenu", "parent")
                                            .Add(Restrictions.Eq("parent.Id", menuId))
                                            .List<Menu>();
                return childMenuItems.Count > 0;

            }
        }

        public Dictionary<string, object> GetEventList()
        {
            return EventService.EventMenuList.Where(e => e.Value.ActionType != EventType.InputDataView)
                                           .Where(m => m.Value.AllowInMenu == true)
                                           .Where(m => !String.IsNullOrWhiteSpace(m.Value.Description))
                                           .OrderBy(m => m.Value.Description)
                                           .ToDictionary(m => m.Key.ToString(), m => (object)m.Value.Description);
        }

        public void DeleteMenuWithId(string menuId)
        {
            using (var session = DataService.OpenSession())
            {
                DeleteChildMenus(menuId, session);

                var menu = session.Get<Menu>(menuId);
                DataService.TryDelete(session, menu);

                session.Flush();
            }
        }

        public void DeleteChildMenus(string menuId, ISession session)
        {
            var childMenuItems = session.CreateCriteria<Menu>()
                                            .CreateAlias("ParentMenu", "parent")
                                            .Add(Restrictions.Eq("parent.Id", menuId))
                                            .List<Menu>();
            foreach (var childMenu in childMenuItems)
            {
                DeleteChildMenus(childMenu.Id, session);
                DataService.TryDelete(session, childMenu);
            }
        }
    }
}