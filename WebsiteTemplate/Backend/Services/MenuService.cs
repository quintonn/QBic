using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class MenuService
    {
        private DataService DataService { get; set; }
        private EventService EventService { get; set; }

        public MenuService(DataService dataService, EventService eventService)
        {
            DataService = dataService;
            EventService = eventService;
        }

        public Menu RetrieveMenu(string menuId)
        {
            using (var session = DataService.OpenSession())
            {
                var result = session.Get<Menu>(menuId);
                return result;
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

        public void DeleteMenu(string menuId)
        {
            using (var session = DataService.OpenSession())
            {
                DeleteChildMenus(menuId, session);

                var menu = session.Get<Menu>(menuId);
                DataService.TryDelete(menu);
                session.Flush();
            }
        }

        private void DeleteChildMenus(string menuId, ISession session)
        {
            var childMenuItems = session.CreateCriteria<Menu>()
                                            .CreateAlias("ParentMenu", "parent")
                                            .Add(Restrictions.Eq("parent.Id", menuId))
                                            .List<Menu>();
            foreach (var childMenu in childMenuItems)
            {
                DeleteChildMenus(childMenu.Id, session);
                DataService.TryDelete(childMenu);
            }
        }
        public void SaveOrUpdateMenu(string menuId, string parentMenuId, EventNumber eventNumber, string name)
        {
            using (var session = DataService.OpenSession())
            {
                Menu dbMenu;
                if (String.IsNullOrWhiteSpace(menuId))
                {
                    dbMenu = new Menu();
                }
                else
                {
                    dbMenu = session.Get<Menu>(menuId);
                }

                if (!String.IsNullOrWhiteSpace(parentMenuId))
                {
                    dbMenu.ParentMenu = session.Get<Menu>(parentMenuId);
                }
                else
                {
                    dbMenu.ParentMenu = null;
                }

                if (eventNumber != null)
                {
                    dbMenu.Event = eventNumber;
                }
                else
                {
                    dbMenu.Event = null;
                }
                dbMenu.Name = name;

                DataService.SaveOrUpdate(dbMenu);
                session.Flush();
            }
        }

        public Dictionary<string, object> GetEventList()
        {
            return EventService.EventList.Where(e => e.Value.ActionType != EventType.InputDataView)
                                           .Where(m => !String.IsNullOrWhiteSpace(m.Value.Description))
                                           .Where(m => m.Value is ShowView) // TODO: this is not right. can't have 'add xx' in menu at the moment
                                           .OrderBy(m => m.Value.Description)  //    maybe need a setting 'allow in menu' etc
                                           .ToDictionary(m => m.Key.ToString(), m => (object)m.Value.Description);
        }

        public List<Menu> RetrieveMenusWithFilter(string menuId, int currentPage, int linesPerPage, string filter)
        {
            using (var session = DataService.OpenSession())
            {
                var query = CreateMenuListQuery(menuId, filter, session);

                var results = query
                      .Skip((currentPage - 1) * linesPerPage)
                      .Take(linesPerPage)
                      .List<Menu>()
                      .ToList();
                return results;
            }
        }

        public int RetrieveMenusCountWithFilter(string menuId, string filter)
        {
            using (var session = DataService.OpenSession())
            {
                var query = CreateMenuListQuery(menuId, filter, session);

                return query.RowCount();
            }
        }

        private IQueryOver<Menu> CreateMenuListQuery(string menuId, string filter, ISession session)
        {
            var query = session.QueryOver<Menu>();

            if (!String.IsNullOrWhiteSpace(menuId))
            {
                query = query.Where(m => m.ParentMenu.Id == menuId);
            }
            else
            {
                query = query.Where(m => m.ParentMenu == null);
            }

            if (!String.IsNullOrWhiteSpace(filter))
            {
                query = query.WhereRestrictionOn(x => x.Name).IsLike(filter, MatchMode.Anywhere);
            }

            return query;
        }
    }
}