using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Menus
{
    public class IncrementMenuOrder : DoSomething
    {
        private DataService DataService { get; set; }

        public IncrementMenuOrder(DataService dataService)
        {
            DataService = dataService;
        }

        public override string Description
        {
            get
            {
                return "Increment Menu Order";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.IncrementMenuOrder;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var menuId = GetValue("Id");

            var parentId = String.Empty;

            using (var session = DataService.OpenSession())
            {
                var menu = session.Get<Menu>(menuId);
                parentId = menu.ParentMenu?.Id;
                Menu menuToReplace;
                if (menu.ParentMenu != null)
                {
                    menuToReplace = session.QueryOver<Menu>().Where(m => m.ParentMenu.Id == menu.ParentMenu.Id && m.Position == menu.Position - 1).SingleOrDefault();
                }
                else
                {
                    menuToReplace = session.QueryOver<Menu>().Where(m => m.ParentMenu == null && m.Position == menu.Position - 1).SingleOrDefault();
                }
                if (menuToReplace != null)
                {
                    menuToReplace.Position += 1;
                    DataService.SaveOrUpdate(session, menuToReplace);
                }
                menu.Position -= 1;
                DataService.SaveOrUpdate(session, menu);
                session.Flush();
            }

            return new List<IEvent>()
            {
                new ExecuteAction(EventNumber.ViewMenus, parentId)
            };
        }
    }
}