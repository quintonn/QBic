using NHibernate;
using NHibernate.Criterion;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Processing.InputProcessing;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.UIProcessors
{
    public class MenuProcessor : NHibernateDataItemService<Menu>
    {
        private MenuService MenuService { get; set; }
        public MenuProcessor(DataService dataService, MenuService menuService)
            : base(dataService)
        {
            MenuService = menuService;
        }

        public override ProcessingResult PreDeleteActivities(ISession session, string itemId)
        {
            MenuService.DeleteChildMenus(itemId, session);
            var menu = session.Get<Menu>(itemId);
            if (menu.ParentMenu == null)
            {
                var otherMenus = session.QueryOver<Menu>().Where(m => m.ParentMenu == null && m.Position > menu.Position).List<Menu>();
                foreach (var m in otherMenus)
                {
                    m.Position--;
                    DataService.SaveOrUpdate(session, m);
                }
            }
            else
            {
                var otherMenus = session.QueryOver<Menu>().Where(m => m.ParentMenu.Id == menu.ParentMenu.Id && m.Position > menu.Position).List<Menu>();
                foreach (var m in otherMenus)
                {
                    m.Position--;
                    DataService.SaveOrUpdate(session, m);
                }
            }
            return new ProcessingResult(true);
        }

        public override Menu RetrieveExistingItem(ISession session)
        {
            return null;
        }

        public override async Task<ProcessingResult> UpdateItem(Menu item)
        {
            var parentMenuId = GetValue("ParentMenuId");
            var eventNumber = GetValue<int?>("Event");
            var name = GetValue("Name");

            if (!String.IsNullOrWhiteSpace(parentMenuId))
            {
                item.ParentMenu = MenuService.RetrieveMenuWithId(parentMenuId);
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

            if (String.IsNullOrWhiteSpace(item.Id))
            {
                using (var session = DataService.OpenSession())
                {
                    if (!String.IsNullOrWhiteSpace(parentMenuId))
                    {
                        var lastMenu = session.QueryOver<Menu>().Where(m => m.ParentMenu.Id == parentMenuId).OrderBy(m => m.Position).Asc().List<Menu>().LastOrDefault();
                        item.Position = lastMenu != null ? lastMenu.Position + 1 : 0;
                    }
                    else
                    {
                        var lastMenu = session.QueryOver<Menu>().Where(m => m.ParentMenu == null).OrderBy(m => m.Position).Asc().List<Menu>().LastOrDefault();
                        item.Position = lastMenu != null ? lastMenu.Position + 1 : 0;
                    }
                }
            }

            return new ProcessingResult(true);
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

            query = query.OrderBy(m => m.Position).Asc();

            return query;
        }
    }
}