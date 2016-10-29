using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Menus
{
    public class DeleteMenu : DeleteItemUsingInputProcessor<MenuProcessor, Menu>
    {
        private MenuService MenuService { get; set; }
        public override string Description
        {
            get
            {
                return "Deletes a menu";
            }
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override EventNumber ViewToShowAfterModify
        {
            get
            {
                return EventNumber.ViewMenus;
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.DeleteMenu;
        }

        public DeleteMenu(MenuProcessor menuProcessor, MenuService menuService)
            : base(menuProcessor)
        {
            MenuService = menuService;
        }

        public override string ParametersToPassToViewAfterModify
        {
            get
            {
                return ParentId;
            }
        }

        private string ParentId { get; set; }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            var confirmed = GetValue<bool>("Confirmation");

            var parentId = String.Empty;

            var menu = ItemProcessor.RetrieveItem(id);
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

            ParentId = parentId;

            return await base.ProcessAction();
        }
    }
}