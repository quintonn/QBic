using NHibernate;
using NHibernate.Criterion;
using System;
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

        public override ProcessingResult DeleteItem(ISession session, string itemId)
        {
            MenuService.DeleteMenuWithId(itemId);
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

            return query;
        }
    }
}