using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class EditMenu : ModifyMenu
    {
        public EditMenu(MenuService menuService, MenuProcessor menuProcessor, DataService dataService)
            : base(menuProcessor, false, menuService, dataService)
        {
        }

        public override EventNumber GetId()
        {
            return EventNumber.EditMenu;
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }
    }
}