using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;
using WebsiteTemplate.SiteSpecific;


namespace WebsiteTemplate.Backend.Menus
{
    public class DeleteMenu : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Deletes a menu";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteMenu;
        }

        public override async Task<IList<Event>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            var confirmed = GetValue<bool>("Confirmation");

            var parentId = String.Empty;

            var menu = MenuService.RetrieveMenu(id);
            parentId = menu.ParentMenu == null ? String.Empty : menu.ParentMenu.Id;

            var isParentMenu = MenuService.IsParentMenu(id);
            if (isParentMenu && !confirmed)
            {
                var tmpData = new Dictionary<string, object>(InputData);
                tmpData.Add("Confirmation", true);

                return new List<Event>()
                    {
                        new UserConfirmation("Menu has sub-menus.\nThey will be deleted as well?")
                        {
                            OnConfirmationUIAction = EventNumber.DeleteMenu,
                            CancelButtonText = "Cancel",
                            ConfirmationButtonText = "Ok",
                            Data = tmpData
                        }
                    };
            }

            MenuService.DeleteMenu(menu.Id);

            return new List<Event>()
            {
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewMenus, parentId),
                new ShowMessage("Menu deleted successfully"),
            };
        }
    }
}