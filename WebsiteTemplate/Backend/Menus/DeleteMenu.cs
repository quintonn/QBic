using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;


namespace WebsiteTemplate.Backend.Menus
{
    public class DeleteMenu : DoSomething  TODO: Make a delete processor thing
    {
        private MenuService MenuService { get; set; }
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

        public DeleteMenu(MenuService service)
        {
            MenuService = service;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            var confirmed = GetValue<bool>("Confirmation");

            var parentId = String.Empty;

            var menu = MenuService.RetrieveMenuWithId(id);
            parentId = menu.ParentMenu == null ? String.Empty : menu.ParentMenu.Id;

            var isParentMenu = MenuService.IsParentMenu(id);
            if (isParentMenu && !confirmed)
            {
                var tmpData = new Dictionary<string, object>(InputData);
                tmpData.Add("Confirmation", true);

                return new List<IEvent>()
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

            MenuService.DeleteMenuWithId(menu.Id);

            return new List<IEvent>()
            {
                new CancelInputDialog(),
                new ExecuteAction(EventNumber.ViewMenus, parentId),
                new ShowMessage("Menu deleted successfully"),
            };
        }
    }
}