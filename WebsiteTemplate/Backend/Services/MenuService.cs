using NHibernate;
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
    public class MenuService : NHibernateDataItemService<Menu>
    {
        private EventService EventService { get; set; }

        public MenuService(DataService dataService, EventService eventService)
            : base(dataService)
        {
            EventService = eventService;
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

        public override void DeleteItem(ISession session, string itemId)
        {
            DeleteChildMenus(itemId, session);

            var menu = session.Get<Menu>(itemId);
            DataService.TryDelete(menu);
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

        public override Menu RetrieveExistingItem(ISession session)
        {
            return null;
        }

        public override void UpdateItem(ISession session, Menu item)
        {
            var parentMenuId = GetValue("ParentMenuId");
            var eventNumber = GetValue<int?>("Event");
            var name = GetValue("Name");

            if (!String.IsNullOrWhiteSpace(parentMenuId))
            {
                item.ParentMenu = session.Get<Menu>(parentMenuId);
            }
            else
            {
                item.ParentMenu = null;
            }

            if (eventNumber != null)
            {
                item.Event = eventNumber;
            }
            else
            {
                item.Event = null;
            }
            item.Name = name;
        }

        public Dictionary<string, object> GetEventList()
        {
            return EventService.EventList.Where(e => e.Value.ActionType != EventType.InputDataView)
                                           .Where(m => !String.IsNullOrWhiteSpace(m.Value.Description))
                                           .Where(m => m.Value is ShowView) // TODO: this is not right. can't have 'add xx' in menu at the moment
                                           .OrderBy(m => m.Value.Description)  //    maybe need a setting 'allow in menu' etc
                                           .ToDictionary(m => m.Key.ToString(), m => (object)m.Value.Description);
        }

        public override IQueryOver<Menu, Menu> CreateQueryForRetrieval(IQueryOver<Menu, Menu> query, string filter, IDictionary<string, object> additionalParameters)
        {
            var menuId = InputProcessingMethods.GetValue<string>(additionalParameters, "MenuId");
            
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