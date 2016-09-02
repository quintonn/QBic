using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class EditMenu : ModifyMenu
    {
        public EditMenu(MenuService menuService, MenuProcessor menuProcessor)
            : base(menuProcessor, false, menuService)
        {
        }

        public override EventNumber GetId()
        {
            return EventNumber.EditMenu;
        }
    }
}