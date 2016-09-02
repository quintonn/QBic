using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Backend.UIProcessors;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.Menus
{
    public class AddMenu : ModifyMenu
    {
        public AddMenu(MenuService menuService, MenuProcessor menuProcessor)
            : base(menuProcessor, true, menuService)
        {

        }

        public override EventNumber GetId()
        {
            return EventNumber.AddMenu;
        }
    }
}